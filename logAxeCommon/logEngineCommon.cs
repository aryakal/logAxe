//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace logAxeCommon
{

   public class WebFrame
   {
      public WebFrame() { 
      }
      public WebFrame(string name, LogFrame logFrame)
      {
         ViewName = name;
         SetFrameAndFilter(logFrame, new TermFilter());
      }

      public void SetFrame(LogFrame logFrame)
      {
         Frame = logFrame;
         TotalLogLines = logFrame.TotalLogLines;
         SystemTotalLogLine = logFrame.SystemTotalLogLine;
         Error = logFrame.LogTypeLength(LogType.Error);
         Info = logFrame.LogTypeLength(LogType.Info);
         Warning = logFrame.LogTypeLength(LogType.Warning);
         Trace = logFrame.LogTypeLength(LogType.Trace);
         TotalLogFiles = logFrame.TotalLogFiles;
         UpdateVersion();
      }

      public void SetFrameAndFilter(LogFrame logFrame, TermFilter filter)
      {
         Filter = filter;
         SetFrame(logFrame);
      }
      // TODO : remove Newtonsoft.Json
      //[JsonProperty("n")]
      public string ViewName { get; set; }
      public int SystemTotalLogLine { get; set; }
      public int TotalLogLines { get; set; }
      public int Error { get; set; }
      public int Info { get; set; }
      public int Warning { get; set; }
      public int Trace { get; set; }

      public int TotalLogFiles { get; set; }
      
      [JsonIgnore]
      public LogFrame Frame { get; set; }
      public TermFilter Filter { get; set; }
      
      
      public int Version { get; set; }

      public void UpdateVersion()
      {
         if (Version >= 10)
         {
            Version = 0;
         }
         else
         {
            Version += 1;
         }

      }

   }

   public class Command
   {
      // TODO : remove Newtonsoft.Json
      //[JsonProperty("op")]
      public string Operation { get; set; }
      public string[] Paths { get; set; }
   }



   public class DirItem
   {
      [JsonProperty("p")]
      public string Path { get; set; }
      [JsonProperty("n")]
      public string Name { get; set; }
   }

   public class WebLogLines
   {
      [JsonProperty("n")]
      public string ViewName { get; }

      public int StartLogLine { get; set; }

      public List<LogLine> LogLines { get; set; }
      public WebLogLines(string name, int capacity)
      {
         ViewName = name;
         LogLines = new List<LogLine>(capacity);
      }

      public WebLogLines() { 
      }
   }
   public class UploadedFile
   {
      public string Name { get; set; }
      public int Size { get; set; }
      public string Data { get; set; }
   }

   class ViewData
   {
      public LogFrame Frame { get; set; } = null;
      public TermFilter Filter { get; set; } = new TermFilter();
   }

   
}
