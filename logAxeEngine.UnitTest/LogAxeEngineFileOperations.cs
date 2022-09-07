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
using logAxeCommon.Interfaces;
using logAxeCommon.Files;
using logAxePlugin;
using logAxeEngine.Common;

namespace logAxeEngine.UnitTest
{
   [TestClass]
   public class LogAxeEngineFileOperations
   {
      ILogEngine _sut;

      [TestInitialize]
      public void Setup()
      {
         var pluginManager = new PluginManager();         
         _sut = new LogAxeEngineManager(pluginManager);
         var goodPlugin = new UnitTestLogParser();
         var errorPlugin = A.Fake<ILogParser>();
         A.CallTo(() => errorPlugin.CanParseLogFile(A<string>.That.EndsWith("fail.txt"))).Returns(true);
         A.CallTo(() => errorPlugin.ParseFile(A<IParsedLogStore>.Ignored)).Throws<NotImplementedException>();
         A.CallTo(() => errorPlugin.ParserName).Returns("exceptionParser");
         pluginManager.LoadPlugin(new UnitTestLogParser());
         pluginManager.LoadPlugin(errorPlugin);
      }



      [TestMethod]

      public void TestBadParsedFile()
      {
         _sut.Clear();

         _sut.AddFiles(new IFileObject[] { 
            TestCommon.GetFakeFile(ext:"fail.txt"), 
            TestCommon.GetFakeFile(),
            TestCommon.GetFakeFile(ext:"fail.txt"),
         }, processAsync: false, addFileAsync: false);
         
         var infos = _sut.GetAllLogFileInfo();
         var result = new bool[] { false, true, false };
         Assert.IsTrue(infos.Length == result.Length, $"Expected {result.Length} files but got {infos.Length}");
         for (int ndx = 0; ndx < result.Length; ndx++) {
            Assert.IsTrue(infos[ndx].IsLoaded == result[ndx], $"Expected for {ndx} Loaded: {result[ndx]} != {infos[ndx].IsLoaded}");
         }


         _sut.AddFiles(new IFileObject[] {
            TestCommon.GetFakeFile(),
         }, processAsync: false, addFileAsync: false);

         infos = _sut.GetAllLogFileInfo();
         result = new bool[] { false, true, false, true};
         Assert.IsTrue(infos.Length == result.Length, $"Expected {result.Length} files but got {infos.Length}");
         for (int ndx = 0; ndx < result.Length; ndx++)
         {
            Assert.IsTrue(infos[ndx].IsLoaded == result[ndx], $"Expected for {ndx} Loaded: {result[ndx]} != {infos[ndx].IsLoaded}");
         }
      }

      [TestMethod]
      public void TestNoParserAvaliable()
      {
         _sut.Clear();
         _sut.AddFiles(new IFileObject[] {
            TestCommon.GetFakeFile(ext:"noparser.txt")
         }, processAsync: false, addFileAsync: false);

         var infos = _sut.GetAllLogFileInfo();
         var result = new bool[] { false };
         Assert.IsTrue(infos.Length == result.Length, $"Expected {result.Length} files but got {infos.Length}");
         for (int ndx = 0; ndx < result.Length; ndx++)
         {
            Assert.IsTrue(infos[ndx].IsLoaded == result[ndx], $"Expected for {ndx} Loaded: {result[ndx]} != {infos[ndx].IsLoaded}");
         }


         
      }
   }
}
