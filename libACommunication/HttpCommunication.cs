//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Net;
using System.Text;
using System.IO;

using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using libALogger;

namespace libACommunication
{
   public static class WebHelper
   {
      public static readonly string RespSuccess = "Success";
      public static readonly string RespFailed = "Failed";
      public static readonly string RespNone = "None";

      public static void SendFile(HttpListenerContext ctx, string filePath)
      {
         using (var writer = new BinaryWriter(ctx.Response.OutputStream))
         {
            writer.Write(File.ReadAllBytes(filePath));
         }
      }
      public static void SendJson(HttpListenerContext ctx, string json)
      {
         var buffer = Encoding.ASCII.GetBytes(json);
         ctx.Response.ContentType = "Application/json";
         ctx.Response.ContentLength64 = buffer.Length;
         using (var writer = new BinaryWriter(ctx.Response.OutputStream))
         {
            writer.Write(buffer);
         }
      }

      public static void SendResponse(HttpListenerContext ctx, string data)
      {
         var buffer = Encoding.ASCII.GetBytes(data);
         ctx.Response.ContentLength64 = buffer.Length;
         using (var writer = new BinaryWriter(ctx.Response.OutputStream))
         {
            writer.Write(buffer);
         }
      }

      public static string GetPostData(HttpListenerContext ctx)
      {
         using (var reader = new StreamReader(ctx.Request.InputStream,
                              ctx.Request.ContentEncoding))
         {
            return reader.ReadToEnd();
         }

      }
   }

   /// <summary>
   /// Creating a generic webserver handling simple page and websocket.
   /// </summary>
   public class LibWebServer : IDLServer
   {
      private int _port;
      private string _address;

      private bool _keepRunning = true;
      private Task _internalServerTsk;
      private ManualResetEvent _keepRunningWaitHandle;
      private CancellationTokenSource _broadcastCts = new CancellationTokenSource();

      private ILibALogger _logger;
      private ILibALogger _websoketLogger;
      private IProtoProcessorProcessClientsAdv<HttpListenerContext> _processor;
      PDHelper<WebSocketEntity> _pDHelper = new PDHelper<WebSocketEntity>();

      public LibWebServer(IProtoProcessorProcessClientsAdv<HttpListenerContext> processor, string address, int port, ILibALogger logger = null, ILibALogger websoketLogger = null)
      {
         _processor = processor;
         _address = address;
         _port = port;
         _logger = logger;
         _websoketLogger = websoketLogger;
      }
      
      public void Stop()
      {
         _logger?.Info("Stopping the services");
         _keepRunning = false;
         _keepRunningWaitHandle?.Set();
         _broadcastCts.Cancel();
         _internalServerTsk?.Wait();
         _processor = null;
         _logger?.Info("Stopping the services");
      }
      private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
      {
         e.Cancel = true;
         Stop();
      }

      public Task Run(CancellationToken token)
      {
         _internalServerTsk = Task.Factory.StartNew(() =>
         {
            try
            {
               using (_keepRunningWaitHandle = new ManualResetEvent(!_keepRunning))
               using (var listener = new HttpListener())
               {
                  Console.CancelKeyPress += Console_CancelKeyPress;
                  listener.Prefixes.Add($@"http://{_address}:{_port}/");
                  listener.Start();
                  _logger?.Info($"started server http://{_address}:{_port}/");
                  while (_keepRunning)
                  {
                     try
                     {

                        var contextAsyncResult = listener.BeginGetContext((IAsyncResult asyncResult) =>
                        {
                           try
                           {

                              var ctx = listener.EndGetContext(asyncResult);
                              if (!ctx.Request.IsWebSocketRequest)
                              {
                                 _logger?.Debug($"{ctx.Request.HttpMethod.PadLeft(4)}, {ctx.Request.RawUrl}");
                                 _processor.ProcessUnitCmd(ctx);                                 
                              }
                              else
                              {
                                 try
                                 {
                                    _logger?.Debug($"ws, {ctx.Request.RawUrl}");
                                    var wsContext = ctx.AcceptWebSocketAsync(subProtocol: null).Result;
                                    var webClient = new WebSocketEntity(
                                        id: SimpleClientInfogGenerator.Generate(),
                                        webSocket: wsContext.WebSocket,
                                        requestProcessor: this,
                                        logger: _websoketLogger
                                        );
                                    _pDHelper.AddClient(webClient.ID, webClient);
                                    _ = webClient.Run(token);

                                 }
                                 catch (Exception)
                                 {
                                    // server error if upgrade from HTTP to WebSocket fails
                                    ctx.Response.StatusCode = 500;
                                    ctx.Response.Close();
                                    return;
                                 }
                              }
                           }
                           catch (Exception ex)
                           {
                              _logger?.Error(ex.ToString());
                           }
                        },
                        null
                        );
                        WaitHandle.WaitAny(new WaitHandle[] { contextAsyncResult.AsyncWaitHandle, _keepRunningWaitHandle });
                     }
                     catch (Exception ex)
                     {
                        _logger?.Error(ex.ToString());
                     }
                  }
               }
            }
            catch (Exception)
            {
               _logger?.Error("Error in http server");
            }

         });
         return _internalServerTsk;
      }

      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }

      public Task Send(IClientInfo id, UnitMsg msg)
      {
         return _pDHelper.Clients[id.UniqueId].Send(msg);
      }

      public Task BroadCast(UnitMsg msg)
      {
         return Task.Run(async () =>
         {
            foreach (var info in _pDHelper.Clients)
            {
               await info.Value.Send(msg);
            }
         });
      }

      public UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null)
      {
         //_logger?.Debug($"|{message.UID}|{message.OpCode}|");
         switch (msgType)
         {
            case LibCommProtoMsgType.Connected:               
               _logger?.Debug($"Connection Open, {clientInfo.UniqueId}, Clients {_pDHelper.TotalClients}");
               _processor.TotalClients(_pDHelper.TotalClients);
               return null;
            case LibCommProtoMsgType.Disconnected:               
               _pDHelper.RemoveClient(clientInfo);
               _logger?.Debug($"Connecton close, {clientInfo.UniqueId}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
               _processor.TotalClients(_pDHelper.TotalClients);
               return null;
         }
         if (string.IsNullOrEmpty(message.UniqueId))
         {
            message.UniqueId = clientInfo.UniqueId;
         }
         var ret = _processor.ProcessUnitCmd(msgType, clientInfo, message);
         if (ret != null)
         {
            if (message.OpCode != "lines")
               _logger?.Debug($"{ret.Status}|{message.UniqueId}|{message.OpCode}|");
         }
         return ret;
      }
   }

   public class WebSocketEntity : IPLCommunication
   {
      private WebSocket _webSocket;
      private CancellationToken _cancelToken;
      private IProtoProcessorCommand _requestProcessor;
      private byte[] _buffer;
      private ILibALogger _logger;
      public IClientInfo ID { get; }

      public WebSocketEntity(
         IClientInfo id,
         WebSocket webSocket,
         IProtoProcessorCommand requestProcessor,
         ILibALogger logger = null,
         int bufferSize = 4096)
      {
         ID = id;
         _logger = logger;
         _requestProcessor = requestProcessor;
         _webSocket = webSocket;
         _buffer = new byte[bufferSize];
      }

      public async Task Send(string msg)
      {
         if (_webSocket.State == WebSocketState.Open)
            await _webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, _cancelToken).ConfigureAwait(false);
         else
            _logger?.Error($"Trying to send on closed socket, {ID.UniqueId}");
      }

      public async Task Send(UnitMsg operation)
      {
         if (null != operation)
         {
            var result = JsonConvert.SerializeObject(operation, Formatting.None);
            _logger?.Debug(result);
            await Send(result);
            await Send(JsonConvert.SerializeObject(operation, Formatting.Indented));
         }
      }

      private async Task ProcessResponse(string msg)
      {
         try
         {
            var message = JsonConvert.DeserializeObject<UnitMsg>(msg);
            //_logger?.Debug($"|{message.UID}|{message.OpCode}|");
            await Send(_requestProcessor.ProcessUnitCmd(LibCommProtoMsgType.Msg, ID, message));
         }
         catch (Exception)
         {
            _logger.Debug("Error in processing request");
         }
      }
      
      public Task Run(CancellationToken token)
      {
         return Task.Run(async () =>
         {
            _requestProcessor?.ProcessUnitCmd(LibCommProtoMsgType.Connected, ID);
            try
            {
               while (_webSocket.State == WebSocketState.Open && !_cancelToken.IsCancellationRequested)
               {
                  var receiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(_buffer), _cancelToken);
                  if (receiveResult.MessageType == WebSocketMessageType.Close)
                  {
                     await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cancelToken);
                     break;
                  }

                  await ProcessResponse(Encoding.UTF8.GetString(_buffer, 0, receiveResult.Count));
               }
            }
            catch (OperationCanceledException)
            {
               // normal upon task/token cancellation, disregard
            }
            catch (Exception ex)
            {
               _logger?.Error(ex.ToString());
            }
            finally
            {
               _webSocket?.Dispose();
            }
            _logger?.Debug("Closing the ws client");
            _requestProcessor?.ProcessUnitCmd(LibCommProtoMsgType.Disconnected, ID);
            _requestProcessor = null;
         });
      }

      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }
   }
}
