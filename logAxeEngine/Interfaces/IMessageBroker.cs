//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeEngine.Interfaces
{
   public enum LogAxeMessageEnum
   {
      FileParseStart,
      FileParseProgress,
      FileParseEnd,
      EngineOptmizeComplete,
      NewViewAnnouncement,
      ResourceUsage,
      FileExportProgress,
      BroadCastGlobalLine,
      NewUserConfiguration,
      AddLineToNotepadEvent,
      NotepadAddedOrRemoved,
      NewMainFrmAddRemoved,
      FilterChanged,
      AwakeAllWindows,
   }

   public interface ILogAxeMessage
   {
      string FromClientID { get; set; }
      LogAxeMessageEnum MessageType { get; set; }
   }

   public class LogAxeGenericMessage : ILogAxeMessage
   {
      public string FromClientID { get; set; }
      public LogAxeMessageEnum MessageType { get; set; }
   }

   public interface IMessageBroker
   {
      void Start();
      void Stop();
      void SendMessage(ILogAxeMessage message);
      string RegisterClient(IMessageReceiver receiver);
      void UnregisterClient(string id);
   }

   public interface IMessageReceiver
   {
      void GetMessage(ILogAxeMessage message);
   }
}
