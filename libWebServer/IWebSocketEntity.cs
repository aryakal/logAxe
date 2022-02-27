//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Threading.Tasks;

namespace libWebServer
{
   public delegate void WebSocketEvent(ISocketID id);
   public delegate void WebSocketMsgEvent(ISocketID id, string msg);
   public interface IWebSocketEntity
   {      
      Task Send(string msg);
      Task ProcessWebSocket();
      event WebSocketEvent OnClose;
      event WebSocketMsgEvent OnNewMessage;
      ISocketID ID { get; }
   }

   public enum WebSocketMsgType { 
      Connected,
      Disconnected,
      Msg
   }
}
