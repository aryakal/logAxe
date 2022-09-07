//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;

namespace logAxeEngine.EventMessages
{
   public class AddLineToNotepadEvent
   {
      public string FromClientID { get; set; }
      public string NotebookName { get; set; }
      public int [] GlobalLine { get; set; }
   }
}
