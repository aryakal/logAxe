//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;

namespace logAxeEngine.EventMessages
{

   public class CurrentResourceUsage : ILogAxeMessage
   {
      public string FromClientID { get; set; }
      public LogAxeMessageEnum MessageType { get; set; } = LogAxeMessageEnum.ResourceUsage;
      public string TotalAppMemUsage { get; set; }
      public string CurrentFileTotalSize { get; set; }
      public string TotalFiles { get; set; }

   }
}
