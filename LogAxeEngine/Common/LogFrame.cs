using logAxeCommon;
using System;
using System.Linq;

namespace logAxeEngine.Common
{
    /// <summary>
    /// This presensts a view/sliece of the whole log lines in the system
    /// Here we have a list of in (Data) which contains the global log line    
    /// </summary>
    public class LogFrame
    {
        public enum FrameType
        {
            Empty,
            Main,
            Filtered
        }

        public int SystemTotalLogLine { get; private set; }
        public int TotalLogLines { get; private set; }
        private int[] Data { get; set; }
        private int[] LogTypeSizes { get; set; } = new int[4];
        public LogType[] LogTypes { get; set; }
        public int LogTypeLength(LogType logType)
        {
            return LogTypeSizes[(int)logType];
        }

        private bool _isMainView;
        private FrameType Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="total">Total file in the system so that it can be shown on the UI</param>
        /// <param name="size">Current size of the frame</param>
        /// <param name="logTypes">Total log line length.</param>
        /// <param name="logLengths">total log lengths</param>
        /// <param name="data"></param>
        public LogFrame(int total, int size, LogType[] logTypes, int[] logLengths, int[] data = null)
        {
            SystemTotalLogLine = total;
            TotalLogLines = size;
            LogTypes = logTypes;
            LogTypeSizes = logLengths == null ? new int[4] { 0, 0, 0, 0 } : logLengths;
            Type = FrameType.Filtered;
            Data = data;

            if (LogTypes == null && LogTypeSizes == null && data == null)
            {
                Type = FrameType.Empty;
            }
            else if (data == null)
            {
                Type = FrameType.Main;
                _isMainView = true;
            }
        }

        public LogFrame GetCloneCopy()
        {
            return new LogFrame(
                SystemTotalLogLine,
                TotalLogLines,
                LogTypes?.ToArray(),
                LogTypeSizes?.ToArray(),
                Data?.ToArray());
        }

        /// <summary>
        /// The translate the line to the global line. 
        /// Just save few memory bytes.
        /// </summary>
        /// <param name="logLineNo"> translate  </param>
        /// <returns></returns>
        public int TranslateLine(int logLineNo)
        {
            return _isMainView ? logLineNo : Data[logLineNo];
        }

        public int GetGlobalLine(int line)
        {
            if (Data == null)
            {
                return line;
            }
            return Array.BinarySearch(Data, line);
        }
    }
}
