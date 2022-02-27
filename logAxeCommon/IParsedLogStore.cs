//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeCommon
{
   /// <summary>
   /// Interface , implemented by the engine.
   /// </summary>
   public interface IParsedLogStore
   {
      /// <summary>
      /// Data in bytes the full log file.
      /// </summary>
      byte[] FileData { get; }

      /// <summary>
      /// log file name.
      /// </summary>
      string FileName { get; }

      /// <summary>
      /// Total no of lines currently in the file.
      /// </summary>
      int TotalLines { get; }

      /// <summary>
      /// Add log line. Following fields are required
      ///     TimeStamp, LogType, Msg.
      /// </summary>
      /// <param name="logLine"></param>
      void AddLogLine(LogLine logLine);

      /// <summary>
      /// Special function to add a stack message to the actual msg.
      /// We can only have one stack message per line ie log line.
      /// </summary>
      /// <param name="lineNo"></param>
      /// <param name="stackMsg"></param>
      void AttachStack(int lineNo, string stackMsg);
   }
}
