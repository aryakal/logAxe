//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;

namespace logAxeEngine.EventMessages
{
   public class CurrentGlobalLine
   {
      public string FromClientID { get; set; }
      public int GlobalLine { get; set; }
   }
}
