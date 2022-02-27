//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Globalization;
using System.Text;
using logAxeCommon;

namespace logAxePlugin
{
   public class GeneralLogParser : ILogParser
   {
      public string ParserName => "generalLogParser";

      public bool CanParseLogFile(string fileName)
      {
         return fileName.ToLower().EndsWith(".txt");
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

            // : LogType,
            // yyyyMMddHHmmssfff: dt,            
            // catgory: string,
            // message: string
            //Trace, dt, thid, cat, msg
            //T,20200130113250200, 1, categor, Msg from id. 180

            logFile.AddLogLine(
                new LogLine()
                {
                   TimeStamp = DateTime.ParseExact(words[1].Trim(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
                   LogType = tp,
                   Tag = words[2],
                   Category = words[3],
                   Msg = words[4],
                   LineNumber = lineNo
                }
                );
         }
      }

      public PluginFeatureSupport GetSupportedFeatures() {
         return new PluginFeatureSupport()
         {
            ProcID = PluginFeatureSupport.SupportType.No,
            ThreadID = PluginFeatureSupport.SupportType.No,
            Tag = PluginFeatureSupport.SupportType.No,
         };
      }
   }
}
