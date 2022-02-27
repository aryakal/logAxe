//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;

namespace logAxeEngine.EventMessages
{
   public class CurrentGlobalLine : ILogAxeMessage
   {
      public string FromClientID { get; set; }
      public LogAxeMessageEnum MessageType { get; set; } = LogAxeMessageEnum.BroadCastGlobalLine;
      public int GlobalLine { get; set; }
   }
}
