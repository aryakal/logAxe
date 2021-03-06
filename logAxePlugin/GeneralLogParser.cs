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
                if (String.IsNullOrEmpty(line) || line.Length < 19)
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

                //T,20200130113250200, 1, 27, category2, Msg from id. 180
                logFile.AddLogLine(
                    new LogLine()
                    {
                        TimeStamp = DateTime.ParseExact(line.Substring(2, 17), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture),
                        LogType = tp,
                        Msg = words[5],
                        LineNumber = lineNo
                    }
                    );
            }
        }
    }
}
