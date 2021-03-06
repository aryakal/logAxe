using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using logAxeEngine.Engines;
using logAxeEngine.Interfaces;


namespace logAxeEngine.UnitTest
{
    [TestClass]
    public class SanityCheck
    {
        [TestCleanup]
        public void testClean()
        {            
        }
        ILogEngine _testEngine;
        [TestInitialize]
        public void testInit()
        {
            var startTime = new DateTime(2020, 1, 30, 11, 30, 30, 340);
            var fileSystem = new UnitTestIOWrapper();
            foreach (var node in GetFiles(2, startTime, 1, 200))
            {
                fileSystem.AddFile("test", node);
            }
            _testEngine = new LogAxeEngineManager(fileSystem);
            _testEngine.RegisterPlugin(".");
            _testEngine.AddFiles(
                paths: new string[] { "test" },
                processAsync: false,
                addFileAsync: false
                );
        }

        [TestMethod]
        public void TestPluginLoading()
        {
            var fileSystem = new UnitTestIOWrapper();
            var engine = new LogAxeEngineManager(fileSystem);
            engine.RegisterPlugin(".");
        }

        [TestMethod]
        public void TestDataSanity()
        {
            var view = _testEngine.GetMasterFrame();            
            var info_license = _testEngine.GetLicenseInfo();
            var data_files = _testEngine.GetAllFileNames();
            Assert.IsTrue(info_license.Contains("(MIT)"));            
            Assert.AreEqual(200  * 2, _testEngine.TotalLogLines);
            Assert.AreEqual(_testEngine.TotalLogLines, view.TotalLogLines);
            Assert.AreEqual(2, data_files.Length);
        }

        [TestMethod]
        public void TestViewData()
        {
            var view = _testEngine.GetMasterFrame();
            var line0 = _testEngine.GetLogLine(view.TranslateLine(0));
            var line399 = _testEngine.GetLogLine(view.TranslateLine(399));
            Assert.AreEqual(400, view.TotalLogLines);
            Assert.AreEqual(0, line0.FileNumber);
            Assert.AreEqual(1, line399.FileNumber);
        }


        public IEnumerable<Node> GetFiles(int TotalFiles, DateTime startTime, int globalLine, int totalLines = 30000)
        {
            var lst = new List<Node>();
            for (int fileNo = 0; fileNo < TotalFiles; fileNo++)
            {
                lst.Add(GetFile(startTime, globalLine, totalLines));
            }
            return lst;
        }

        public Node GetFile(DateTime startTime, int globalLine, int totalLines= 30000)
        {
            string name = startTime.ToString("yyyyMMddHHmmssfff")+".txt";
            string logType = "T";
            string category = "categor1";
            int threadNo = 1;
            string seedMsg = "The quick brown fox jumps over the lazy dog, The quick brown fox jumps over the lazy dog";
            int procNo = 1;
            var timeSpan = new TimeSpan(0, 0, 0, 0, 997);
            var sb = new StringBuilder();
            for (int lineNo = 0; lineNo < totalLines; lineNo++)
            {
                logType = "T";
                startTime += timeSpan;
                globalLine += 1;
                sb.Append($"{logType},{startTime:yyyyMMddHHmmssfff}, {procNo}, {threadNo}, {category}, {globalLine}{seedMsg}\n");
            }
            return new Node(nodeType: Node.FileType, name) { Data = Encoding.UTF8.GetBytes(sb.ToString()) };            
        }
    }

    public class UnitTestIOWrapper : ISystemIO
    {
        public Node ParentNode { get; set; }
        public Dictionary<string, Node> Nodes = new Dictionary<string, Node>();

        public UnitTestIOWrapper()
        {
            ParentNode = new Node(Node.DirType, "test");
            Nodes["test"] = ParentNode;
        }

        public void AddFile(string dirPath, Node fileNode)
        {
            fileNode.FullName = $@"{dirPath}\{fileNode.Name}";
            Nodes[dirPath].ChildNodes.Add(fileNode);
            Nodes[fileNode.FullName] = fileNode;
        }

        public string[] GetFilesInDir(string path)
        {
            var dirNode = Nodes[path];
            var lst = new List<string>();
            foreach (var item in dirNode.ChildNodes)
            {
                if (item.NodeType == Node.FileType)
                {
                    lst.Add(item.FullName);
                }
            }
            return lst.ToArray();
        }

        public long GetFileSize(string path)
        {
            return Nodes[path].Data.Length;
        }

        public bool IsDirectory(string path)
        {
            return Nodes[path].NodeType == Node.DirType;
        }

        public bool IsFile(string path)
        {
            return Nodes[path].NodeType == Node.FileType;
        }

        public byte[] GetData(string path)
        {
            return Nodes[path].Data.ToArray();
        }
    }
    public class Node
    {
        public Node(string nodeType, string name)
        {
            Name = name;            
            NodeType = nodeType;
            
            if (nodeType == FileType)
            {
                ChildNodes = null;
            }
            else
            {
                Data = null;
            }
        }
        public string FullName { get; set; }
        public const string DirType = "Dir";
        public const string FileType = "File";
        public string Name { get; set; }
        public string NodeType { get; }
        public byte[] Data { get; set; }

        public List<Node> ChildNodes = new List<Node>(1);
    }
}
