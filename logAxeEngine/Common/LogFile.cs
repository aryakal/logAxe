//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Collections.Generic;

using logAxeCommon;
using logAxeEngine.Storage;
using logAxeEngine.Interfaces;


namespace logAxeEngine.Common
{
    public class LogFile : IParsedLogStore
    {
        public List<LogLine> LogLines = new List<LogLine>();
        public IStorageString StringStore { get; private set; } = new StorageStringList();
        public IStorageString DBTags { get; private set; } = new StorageStringList();

        public byte[] FileData { get; private set; }
        public string FileName { get; }
        public int TotalLines => LogLines.Count;
        public int FileId { get; set; }

        public LogFile(string fileName, byte[] fileData)
        {
            FileName = fileName;
            FileData = fileData;
        }

        public void Clear()
        {

            FileData = null;

            StringStore.Clear();
            DBTags.Clear();

            StringStore = null;
            DBTags = null;
            LogLines.Clear();
            LogLines = null;
        }

        public LogFile()
        {
            StringStore = new StorageStringList();
            DBTags = new StorageStringList();
        }

        public void AddLogLine(LogLine logLine)
        {
            logLine.MsgId = StringStore.StoreString(logLine.Msg);
            logLine.StackTraceId = StringStore.StoreString(logLine.StackTrace);

            if (logLine.ThreadNo != LogLine.INVALID ||
                logLine.ProcessId != LogLine.INVALID ||
                logLine.Category != "")
            {
                logLine.TagId = DBTags.StoreString($"P:{logLine.ProcessId};T:{logLine.ThreadNo};C:{logLine.Category};");
            }
            else
            {
                logLine.TagId = LogLine.INVALID;
            }


            logLine.Msg = logLine.StackTrace = logLine.Category = "";

            LogLines.Add(logLine);
        }

        public void AttachStack(int lineNo, string stackMsg)
        {
            LogLines[lineNo].StackTraceId = StringStore.StoreString(stackMsg);
        }
    }
}

