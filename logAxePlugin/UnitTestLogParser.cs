//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Globalization;
using System.Text;

using logAxeCommon;
using logAxeCommon.Interfaces;

namespace logAxePlugin
{
   public class UnitTestLogParser : ILogParser
   {
      public string ParserName => "UnitTestLogParser";

      public bool CanParseLogFile(string fileName)
      {
         return fileName.ToLower().EndsWith(".unit.txt");
      }

      public void ParseFile(IParsedLogStore logFile)
      {
         int lineNo = 0;
        
         foreach (var line in Encoding.UTF8.GetString(logFile.FileData).Split(new[] { "\n" }, StringSplitOptions.None))
         {
            if (string.IsNullOrEmpty(line) || line.Length < 19)
            {
               continue;
            }

            var words = line.Split(new[] { "," }, StringSplitOptions.None);

            var tp = LogType.Trace;
            switch (line[0])
            {
               case 'E':
                  tp = LogType.Error;
                  break;

               case 'I':
                  tp = LogType.Info;
                  break;

               case 'W':
                  tp = LogType.Warning;
                  break;
            }

            //Trace, dt, threid, procid, cat, msg 
            //T,20200130113250200, 1, 1, categor, Msg from id. 180

            logFile.AddLogLine(
                new LogLine()
                {
                   TimeStamp = DateTime.ParseExact(words[1].Trim(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
                   LogType = tp,
                   ThreadNo = int.Parse(words[2]),
                   ProcessId= int.Parse(words[3]),                   
                   Category = words[4],
                   Msg = words[5],
                   LineNumber = lineNo
                }
                );
         }
      }

      public PluginFeatureSupport GetSupportedFeatures() {
         return new PluginFeatureSupport()
         {
            ProcID = PluginFeatureSupport.SupportType.Yes,
            ThreadID = PluginFeatureSupport.SupportType.Yes,
            Tag = PluginFeatureSupport.SupportType.Yes,
         };
      }
   }
}
