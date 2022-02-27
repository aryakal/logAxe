//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;

namespace logAxeEngine.EventMessages
{
   public class AddLineToNotepadEvent : ILogAxeMessage
   {
      public string FromClientID { get; set; }
      public string NotebookName { get; set; }
      public LogAxeMessageEnum MessageType { get; set; } = LogAxeMessageEnum.AddLineToNotepadEvent;
      public int [] GlobalLine { get; set; }
   }
}
