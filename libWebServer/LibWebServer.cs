//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace libWebServer
{
   public class LibWebServer : ILibWebServer
   {
      private bool shouldExit = false;
      private ManualResetEvent shouldExitWaitHandle;
      private CancellationTokenSource broadcastTokenSource = new CancellationTokenSource();
      private Dictionary<string, IWebSocketEntity> _clients = new Dictionary<string, IWebSocketEntity>();
      private Task _internalServerTsk;
      private int _socketClientId = 0;
      private string _address;
      private int _port;      
      private ILibWebServerLogger _logger;
      #region Event handler for the Http and WsMessage
      public Action<HttpListenerContext> HttpMessage { get; set; }
      public Action<WebSocketMsgType, ISocketID, string> WsMessage { get; set; }
      #endregion

      public LibWebServer(string address, int port, ILibWebServerLogger logger = null)
      {
         _address = address;
         _port = port;
         _logger = logger;
      }
      public void BroadcastMsg(string msg)
      {
         if (_clients.Count == 0)
         {
            _logger?.Trace($"no clients joined");
            return;
         }
         foreach (var client in _clients)
         {
            _logger?.Trace($"{client.Key}, {msg}");
            client.Value.Send(msg);
         }
      }
      public void SendMsg(ISocketID info, string msg)
      {
         _clients[info.ID].Send(msg);
      }
      public Task Run()
      {
         _internalServerTsk = Task.Factory.StartNew(() => {
            using (shouldExitWaitHandle = new ManualResetEvent(shouldExit))
            using (var listener = new HttpListener())
            {
               Console.CancelKeyPress += Console_CancelKeyPress;
               listener.Prefixes.Add($@"http://{_address}:{_port}/");
               listener.Start();
               _logger?.Trace($"started server http://{_address}:{_port}/");
               while (!shouldExit)
               {
                  try
                  {

                     var contextAsyncResult = listener.BeginGetContext((IAsyncResult asyncResult) =>
                     {
                        try
                        {

                           var ctx = listener.EndGetContext(asyncResult);
                           if (ctx.Request.IsWebSocketRequest)
                           {
                              _logger?.Trace("websocket requested");

                              try
                              {
                                 int socketId = Interlocked.Increment(ref _socketClientId);
                                 var idINfo = new SimpleID($"c{socketId}");
                                 var wsContext = ctx.AcceptWebSocketAsync(subProtocol: null).Result;
                                 WsMessage(WebSocketMsgType.Connected, idINfo, string.Empty);
                                 var webClient = new WebSocketEntity(idINfo, wsContext, broadcastTokenSource.Token);
                                 webClient.OnNewMessage += WebClient_OnNewMessage;
                                 webClient.OnClose += WebClient_OnClose; ;
                                 _clients[idINfo.ID] = webClient;
                                 _ = Task.Run(() => webClient.ProcessWebSocket());
                              }
                              catch (Exception)
                              {
                                 // server error if upgrade from HTTP to WebSocket fails
                                 ctx.Response.StatusCode = 500;
                                 ctx.Response.Close();
                                 return;
                              }
                           }
                           else
                           {
                              HttpMessage(ctx);
                           }
                        }
                        catch (Exception ex)
                        {
                           _logger?.Error(ex.ToString());
                        }

                     },
                     null
                     );
                     WaitHandle.WaitAny(new WaitHandle[] { contextAsyncResult.AsyncWaitHandle, shouldExitWaitHandle });
                  }
                  catch (Exception ex)
                  {
                     Console.Write(ex);
                  }
               }
            }
         });
         return _internalServerTsk;
      }
      public void Stop()
      {
         _logger?.Debug("Stopping the services");
         shouldExit = true;
         shouldExitWaitHandle?.Set();
         broadcastTokenSource.Cancel();
         _internalServerTsk?.Wait();
      }
      private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
      {
         e.Cancel = true;
         Stop();
      }
      private void WebClient_OnClose(ISocketID id)
      {
         _logger?.Trace($"{id.ID}, closed");
         _clients.Remove(id.ID);
         WsMessage(WebSocketMsgType.Disconnected, id, string.Empty);
      }
      private void WebClient_OnNewMessage(ISocketID id, string msg)
      {
         _logger?.Trace($"{id.ID}, {msg}");
         WsMessage(WebSocketMsgType.Msg, id, msg);
      }      
      public void RunForever()
      {
         if (null == _internalServerTsk)
         {
            Run();
         }
         _internalServerTsk?.Wait();
      }

      public void StartWebBrowser()
      {
         var startInfo = new ProcessStartInfo();
         startInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
         startInfo.Arguments = "--new-window --app=http://localhost:8080/logAxe/ui/mainUI.html";
         var process = Process.Start(startInfo);
         process.Exited += Process_Exited;
      }

      private void Process_Exited(object sender, EventArgs e)
      {
         _logger?.Debug($@"Client {e.ToString()} closed");
      }
   }
}
