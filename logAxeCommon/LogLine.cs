//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using Newtonsoft.Json;

namespace logAxeCommon
{
   /// <summary>
   /// Main structure to translate the log file to the internal structure.
   /// </summary>
   public class LogLine
   {
      [JsonProperty("a")]
      public const int INVALID = -1;
      [JsonProperty("b")]
      public int GlobalLine { get; set; } = INVALID;
      [JsonProperty("c")]
      public int FileNumber { get; set; } = INVALID;
      [JsonProperty("d")]
      public int LineNumber { get; set; } = INVALID;
      [JsonProperty("e")]
      public LogType LogType { get; set; }
      [JsonProperty("f")]
      public DateTime TimeStamp { get; set; }
      [JsonProperty("g")]
      public string Msg { get; set; } = "";
      [JsonProperty("h")]
      public string StackTrace { get; set; } = "";
      [JsonProperty("i")]
      public int ThreadNo { get; set; } = INVALID;
      [JsonProperty("j")]
      public int ProcessId { get; set; } = INVALID;
      [JsonProperty("k")]
      public string Category { get; set; } = "";
      [JsonProperty("l")]
      public string Tag { get; set; }

      [JsonProperty("n")]
      /// <summary>
      /// Used internally by the log engine.
      /// </summary>
      public int MsgId { get; set; } = INVALID;
      [JsonProperty("o")]
      /// <summary>
      /// Used internally by the log engine.
      /// </summary>      
      public int StackTraceId { get; set; } = INVALID;
      /// <summary>
      /// Used internally by the log engine.
      /// </summary>
      [JsonProperty("p")]
      public int TagId { get; set; } = INVALID;

   }
}
