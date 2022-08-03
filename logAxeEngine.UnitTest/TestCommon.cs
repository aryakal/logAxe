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
   public static class TestCommon
   {
      public static IFileObject GetFakeFile(string fileName = "test", int totalLines = 100, long timeTicks = 637451028000000000, string ext=".unit.txt")
      {
         fileName += ext;
         var s = new StringBuilder();
         //var t = new DateTime(2021, 1, 1, 13, 0, 0).Ticks;
         var timeStamp = new DateTime(timeTicks);
         var deltaTime = new TimeSpan(0, 1, 0);
         var halfProcess = (int)totalLines / 2;
         var quarterProcess = (int)totalLines / 4;
         var sixth = (int)totalLines / 6;
         //Trace, dt, threid, procid, cat, msg         
         for (var ndx = 0; ndx < totalLines; ndx++)
         {
            timeStamp += deltaTime;
            s.Append($"{GetLogType((LogType)(ndx % 4))},{timeStamp:yyyyMMddHHmmssfff},{(int)ndx / sixth},{(int)ndx / halfProcess}, cat{(int)ndx / quarterProcess}, Msg from id {ndx}\n");
         }

         return new WebFile(
            fileName,
            Encoding.ASCII.GetBytes(s.ToString()
            ));
      }

      private static string GetLogType(LogType tp)
      {
         switch (tp)
         {
            case LogType.Error: return "E";
            case LogType.Trace: return "T";
            case LogType.Info: return "I";
            case LogType.Warning: return "W";
            default: return "";
         }
      }
   }
}
