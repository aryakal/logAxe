//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;

namespace logAxeCommon
{
   /// <summary>
   /// Main structure to translate the log file to the internal structure.
   /// </summary>
   public class LogLine
   {
      public const int INVALID = -1;

      public int GlobalLine { get; set; } = INVALID;
      public int FileNumber { get; set; } = INVALID;
      public int LineNumber { get; set; } = INVALID;

      public LogType LogType { get; set; }

      public DateTime TimeStamp { get; set; }

      public string Msg { get; set; } = "";
      public string StackTrace { get; set; } = "";
      public int ThreadNo { get; set; } = INVALID;
      public int ProcessId { get; set; } = INVALID;
      public string Category { get; set; } = "";
      public string Tag { get; set; }

      /// <summary>
      /// Used internally by the log engine.
      /// </summary>
      public int MsgId { get; set; } = INVALID;
      /// <summary>
      /// Used internally by the log engine.
      /// </summary>
      public int StackTraceId { get; set; } = INVALID;
      /// <summary>
      /// Used internally by the log engine.
      /// </summary>
      public int TagId { get; set; } = INVALID;

   }
}
