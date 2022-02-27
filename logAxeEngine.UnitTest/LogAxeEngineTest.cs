//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using Microsoft.VisualStudio.TestTools.UnitTesting;
using logAxeEngine.Interfaces;
using logAxeEngine.Engines;
using FakeItEasy;
using System.Text;
using System;
using logAxeCommon;
using logAxePlugin;
using logAxeEngine.Common;

namespace logAxeEngine.UnitTest
{
   [TestClass]
   public class LogAxeEngineTest
   {
      ILogEngine _sut;

      [TestInitialize]
      public void Setup()
      {
         var pluginManager = new PluginManager();
         var messageBroker = A.Fake<IMessageBroker>();
         _sut = new LogAxeEngineManager(messageBroker, pluginManager);
         var fakePlugin = new GeneralLogParser();
         pluginManager.LoadPlugin(fakePlugin);

         _sut.AddFiles(new IFileObject[] { GetFakeFile() }, processAsync: false, addFileAsync: false);
      }

      [TestMethod]
      public void TestFileClear() {
         var pluginManager = new PluginManager();
         var messageBroker = A.Fake<IMessageBroker>();
         var localSut = new LogAxeEngineManager(messageBroker, pluginManager);
         var fakePlugin = new GeneralLogParser();
         pluginManager.LoadPlugin(fakePlugin);

         localSut.AddFiles(new IFileObject[] {
            GetFakeFile(timeTicks: new DateTime(2021, 2, 1, 13, 0, 0).Ticks),
            GetFakeFile(timeTicks: new DateTime(2021, 1, 1, 14, 0, 0).Ticks)            
         }, processAsync: false, addFileAsync: false);

         

         Assert.IsTrue(localSut.GetMasterFrame().TotalLogLines == 200);
         Assert.IsTrue(localSut.GetAllFileNames().Length == 2);

         localSut.Clear();

         Assert.IsTrue(localSut.GetMasterFrame().TotalLogLines == 0);
         Assert.IsTrue(localSut.GetAllFileNames().Length == 0);

      }

      [TestMethod]
      
      [DataRow(0, 0, 0, 0, 0, true, true, true, true, "91", "91")]
      [DataRow(0, 0, 0, 0, 0, false, false, false, false, "", "")]
      [DataRow(1, 0, 0, 0, 1, true, true, true, true, "91", "")]
      [DataRow(1, 1, 0, 0, 0, true, true, true, true, "92", "")]
      
      [DataRow(100, 25, 25, 25, 25, true, true, true, true, "", "")]

      [DataRow(2, 2, 0, 0, 0, true, false, false, false, "Msg from id 9", "")]
      [DataRow(11, 2, 3, 3, 3, true, true, true, true, "Msg from id 9", "")]
      [DataRow(3, 0, 0, 0, 3, false, false, false, true, "Msg from id 9", "")]
      [DataRow(3, 0, 0, 3, 0, false, false, true, false, "Msg from id 9", "")]
      [DataRow(3, 0, 3, 0, 0, false, true, false, false, "Msg from id 9", "")]
      

      [DataRow(75, 0, 25, 25, 25, false, true, true, true, "", "")]
      [DataRow(75, 25, 0, 25, 25, true, false, true, true, "", "")]
      [DataRow(75, 25, 25, 0, 25, true, true, false, true, "", "")]
      [DataRow(75, 25, 25, 25, 0, true, true, true, false, "", "")]
      [DataRow(9, 0, 3, 3, 3, false, true, true, true, "Msg from id 9", "")]
      [DataRow(99, 25, 25, 25, 24, true, true, true, true, "", "91")]

      public void CheckMsgEach(
         int totalLines, 

         int errorNo,
         int infoNo,
         int traceNo,
         int warningNo,

         bool errorType,         
         bool infoType,
         bool traceType,
         bool warningType,

         string msgInclude, string msgExclude)
      {
         var term = new TermFilter();
         term.FilterTraces = new [] { errorType, infoType, traceType, warningType };
         term.MsgInclude = msgInclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);
         term.MsgExclude = msgExclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);
         var frame = _sut?.Filter(term);

         Assert.IsTrue(totalLines == frame?.TotalLogLines);
         Assert.IsTrue(errorNo == frame?.LogTypeLength(LogType.Error));
         Assert.IsTrue(infoNo == frame?.LogTypeLength(LogType.Info));
         Assert.IsTrue(traceNo == frame?.LogTypeLength(LogType.Trace));
         Assert.IsTrue(warningNo == frame?.LogTypeLength(LogType.Warning));

      }


      [TestMethod]
      [DataRow(25, 6, 7, 6, 6, "", "", "cat1", "")]      
      public void TestTags(
         int totalLines,
         int errorNo,
         int infoNo,
         int traceNo,
         int warningNo,

         string msgInclude, 
         string msgExclude,
         string tagInclude,
         string tagExclude)
      {
         var term = new TermFilter();      
         term.MsgInclude = msgInclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);
         term.MsgExclude = msgExclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);

         term.TagsInclude = tagInclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);
         term.TagsExclude = tagExclude.Split(new char[';'], StringSplitOptions.RemoveEmptyEntries);

         var frame = _sut?.Filter(term);

         Assert.IsTrue(totalLines == frame?.TotalLogLines);
         Assert.IsTrue(errorNo == frame?.LogTypeLength(LogType.Error));
         Assert.IsTrue(infoNo == frame?.LogTypeLength(LogType.Info));
         Assert.IsTrue(traceNo == frame?.LogTypeLength(LogType.Trace));
         Assert.IsTrue(warningNo == frame?.LogTypeLength(LogType.Warning));

      }

      private IFileObject GetFakeFile(string fileName="test.txt", int totalLines=100, long timeTicks= 637451028000000000)
      {  
         var s = new StringBuilder();
         //var t = new DateTime(2021, 1, 1, 13, 0, 0).Ticks;
         var timeStamp = new DateTime(timeTicks);
         var deltaTime = new TimeSpan(0, 1, 0);         
         var halfProcess = (int)totalLines / 2;
         var quarterProcess = (int)totalLines / 4;
         //Trace, dt, thid, cat, msg         
         for (var ndx = 0; ndx < totalLines; ndx++)
         {            
            timeStamp += deltaTime;
            s.Append($"{GetLogType((LogType)(ndx%4))},{timeStamp:yyyyMMddHHmmssfff}, {(int)ndx/halfProcess}, cat{(int)ndx / quarterProcess}, Msg from id {ndx}\n");            
         }

         return new WebFile(
            fileName,
            Encoding.ASCII.GetBytes(s.ToString()
            ));
      }

      private string GetLogType(LogType tp)
      {
         switch (tp) {
            case LogType.Error: return "E";
            case LogType.Trace: return "T";
            case LogType.Info: return "I";
            case LogType.Warning: return "W";
            default: return "";
         }
      }

   }
}