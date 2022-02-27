//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace libWebServer
{
   public class WebSocketEntity : IWebSocketEntity
   {
      private WebSocket _webSocket;
      private CancellationToken _cancelToken;
      private byte[] _buffer;
      public ISocketID ID { get; }

      public WebSocketEntity(ISocketID id, HttpListenerWebSocketContext ctx, CancellationToken token, int bufferSize = 4096)
      {
         ID = id;
         _cancelToken = token;
         _webSocket = ctx.WebSocket;
         _buffer = new byte[bufferSize];
      }

      public event WebSocketEvent OnClose;
      public event WebSocketMsgEvent OnNewMessage;      
      public async Task Send(string msg)
      {         
         await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, _cancelToken).ConfigureAwait(false);
      }
      public async Task ProcessWebSocket()
      {
         try
         {
            while (_webSocket.State == WebSocketState.Open && !_cancelToken.IsCancellationRequested)
            {
               WebSocketReceiveResult receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), _cancelToken);
               if (receiveResult.MessageType == WebSocketMessageType.Close)
               {
                  await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cancelToken);
               }
               else
               {
                  OnNewMessage?.Invoke(ID, Encoding.UTF8.GetString(_buffer, 0, receiveResult.Count));
               }
            }
         }
         catch (OperationCanceledException)
         {
            // normal upon task/token cancellation, disregard
         }
         catch (Exception)
         {

         }
         finally
         {
            _webSocket?.Dispose();
         }
         OnClose?.Invoke(ID);
      }
   }
}
