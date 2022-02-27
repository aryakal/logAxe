//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Text;
using System.Globalization;
using logAxeCommon;

namespace logAxePlugin
{

   public class EvtxLogParser : ILogParser
   {
      public string ParserName => "evtxLogParser";

      public bool CanParseLogFile(string fileName)
      {
         return fileName.ToLower().EndsWith(".csv");
      }

      public void ParseFile(IParsedLogStore logFile)
      {
         var sbLine = new StringBuilder();
         var lineNo = 0;
         var lines = Encoding.UTF8.GetString(logFile.FileData).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
         foreach (var line in lines)
         {
            if (line.StartsWith("Warning") || line.StartsWith("Information") || line.StartsWith("Error"))
            {
               var wrds = sbLine.ToString().Split(new char[] { ',' });
               if (wrds.Length != 0)
               {
                  var tp = LogType.Trace;
                  switch (wrds[0])
                  {
                     case "Error":
                        tp = LogType.Error;
                        break;

                     case "Information":
                        tp = LogType.Info;
                        break;

                     case "Warning":
                        tp = LogType.Warning;
                        break;

                     default:                        
                        sbLine.Clear();
                        sbLine.Append(line);
                        continue;
                  }
                  logFile.AddLogLine(
                   new LogLine()
                   {
                      //9/24/2021 1:45:35 PM
                      TimeStamp = DateTime.ParseExact(wrds[1], "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                      LogType = tp,
                      Msg = wrds[5],
                      LineNumber = lineNo
                   }                   
                   );
                  lineNo++;

               }
               sbLine.Clear();

            }
            sbLine.Append(line);
         }
      }

      public PluginFeatureSupport GetSupportedFeatures()
      {
         return new PluginFeatureSupport()
         {
            ThreadID = PluginFeatureSupport.SupportType.No,
            Tag = PluginFeatureSupport.SupportType.No,
         };
      }
   }
}
