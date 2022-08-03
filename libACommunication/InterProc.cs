//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using libALogger;
using Newtonsoft.Json;

namespace libACommunication
{
   public class PipeServer : IDLServer
   {
      private ILibALogger _logger;
      private IProtoProcessorProcessClients _processor;
      private PDHelper<PipeServerClientInstace> _pDHelper = new PDHelper<PipeServerClientInstace>();
      private string _pipeName = "";

      public PipeServer(ILibALogger logger, IProtoProcessorProcessClients processor, string pipeName)
      {
         _processor = processor;// TODO CHECK for null.
         _logger = logger;
         _pipeName = pipeName;

      }

      public Task BroadCast(UnitCmd msg)
      {
         return Task.Run(async () =>
         {
            foreach (var info in _pDHelper.Clients)
            {
               await info.Value.Send(msg);
            }
         });
      }

      public UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {
         //switch (msgType)
         //{
         //   case LibCommProtoMsgType.Connected:
         //      _logger?.Debug($"Connection Open, {clientInfo.ID}, Clients {_pDHelper.TotalClients}");
         //      _processor.TotalClients(_pDHelper.TotalClients);
         //      return null;
         //   case LibCommProtoMsgType.Disconnected:
         //      _pDHelper.RemoveClient(clientInfo);
         //      _logger?.Debug($"Connecton close, {clientInfo.ID}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
         //      _processor.TotalClients(_pDHelper.TotalClients);
         //      return null;
         //}
         //if (string.IsNullOrEmpty(message.UID))
         //{
         //   message.UID = clientInfo.ID;
         //}
         switch (msgType)
         {
            case LibCommProtoMsgType.Connected:
               _logger?.Debug($"{msgType}, {clientInfo.ID}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
               _processor.TotalClients(_pDHelper.Clients.Count);
               return null;

            case LibCommProtoMsgType.Disconnected:
               _pDHelper.RemoveClient(clientInfo);
               _logger?.Debug($"{msgType}, {clientInfo.ID}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
               _processor.TotalClients(_pDHelper.Clients.Count);
               return null;

            case LibCommProtoMsgType.Msg:
               return _processor.ProcessUnitCmd(msgType, clientInfo, message);

            default:
               return null;

         }

      }
      public Task Run(CancellationToken token)
      {
         return Task.Run(() =>
         {
            _logger?.Debug($"Starting server waiting for the client to join @ {_pipeName}");
            while (!token.IsCancellationRequested)//TODO remove with cancellation token
            {
               var server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 254, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
               server.WaitForConnection();
               _logger?.Debug("Client connected");
               var id = SimpleClientInfogGenarator.Generate();
               var client = new PipeServerClientInstace(Logging.GetLogger(id.ID), id, server, this);
               _pDHelper.AddClient(client.ID, client);
               _ = client.Run(token);
            }

            _logger?.Debug("RunForever, wait over.");
         });
      }
      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();

      }

      public Task Send(IClientInfo id, UnitCmd msg)
      {
         return _pDHelper.Clients[id.ID].Send(msg);
      }
   }

   public class PipeClient : IProtoProcessorCommand, ILDClient
   {
      ILibALogger _logger;
      IProtoProcessorCommand _processor;
      PipeServerClientInstace _client;
      public PipeClient(ILibALogger logger, IProtoProcessorCommand processor)
      {
         _processor = processor;// TODO CHECK for null.
         _logger = logger;
         _logger?.Debug("Starting the client");
      }

      public UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {

         return _processor.ProcessUnitCmd(msgType, clientInfo, message);
      }

      public Task Run(CancellationToken token)
      {
         var pipe = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.Asynchronous);
         pipe.Connect();
         _client = new PipeServerClientInstace(_logger, null, pipe, this);
         //First get the client id from the server, this will be helpfull later.
         return _client.Run(token);
      }

      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }

      public Task Send(UnitCmd msg)
      {
         return _client.Send(msg);
      }

   }

   public class PipeClientQueued : IProtoProcessorCommand, ILDClient
   {
      private ILibALogger _logger;
      private IProtoProcessorCommand _processor;
      private NamedPipeClientStream _pipe;
      private PipeServerClientInstace _client;
      private BlockingCollection<UnitCmd> _incommingQueue = new BlockingCollection<UnitCmd>();
      private BlockingCollection<UnitCmd> _outgoingQueue = new BlockingCollection<UnitCmd>();
      private string _serverName;
      private Task _tskOutGoingQueue;
      private Task _tskIncommingQueue;

      public PipeClientQueued(ILibALogger logger, IProtoProcessorCommand processor, string serverName)
      {
         _serverName = serverName;
         _processor = processor;// TODO CHECK for null.
         _logger = logger;
         _logger?.Debug("Starting the client");
      }

      public UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {
         if (msgType == LibCommProtoMsgType.Disconnected)
         {
            _pipe?.Close();
         }
         return _processor.ProcessUnitCmd(msgType, clientInfo, message);
      }

      public Task Run(CancellationToken token)
      {
         
         try
         {  
            _pipe = new NamedPipeClientStream(".", _serverName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _logger?.Debug($"Connecting to server {_serverName}");
            _pipe.Connect(5000);
            _logger?.Debug("Connected to server, now creating client");
            _client = new PipeServerClientInstace(_logger, null, _pipe, this);
            _logger?.Debug("Client created");
            _tskIncommingQueue = Task.Run(() =>
            {
               _logger?.Info("Started incomming queue");
               while (!token.IsCancellationRequested)
               {
                  var data = _incommingQueue.Take(token);
                  _processor.ProcessUnitCmd(LibCommProtoMsgType.Msg, null, data);
               }
            });

            _tskOutGoingQueue = Task.Run(async () =>
            {
               _logger?.Info("Started outgoing queue");
               while (!token.IsCancellationRequested)
               {

                  var msg = _outgoingQueue.Take(token);
                  //_logger?.Info($"out : opCode : {msg.OpCode}");
                  await _client.Send(msg);
                  _logger?.Info($"send : opCode : {msg.OpCode}");
               }
            });

            //First get the client id from the server, this will be helpfull later.
            return _client.Run(token);
         }
         catch
         {
            _pipe.Close();
            _logger?.Error("Not able to connect");
            throw;
         }
      }

      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }

      public Task Send(UnitCmd msg)
      {
         _outgoingQueue.Add(msg);
         return Task.CompletedTask;
         //return _client.Send(msg);
      }

   }

   class PipeServerClientInstace : IPLCommunication
   {
      ILibALogger _logger;
      IProtoProcessorCommand _processor;
      PipeStream _stream;
      StreamWriter _writer;
      SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      public IClientInfo ID { get; private set; }


      public PipeServerClientInstace(ILibALogger logger, IClientInfo id, PipeStream stream, IProtoProcessorCommand processor)
      {
         ID = id;
         _logger = logger;
         _stream = stream;
         _processor = processor;
      }

      public async Task Send(string msg)
      {

         await SendOpCode(1, msg);
         //try
         //{
         //   _lock.Wait();
         //   {

         //      var buf = System.Text.Encoding.UTF8.GetBytes(msg);
         //      var packetSize = BitConverter.GetBytes(buf.Length);
         //      var header = new byte[8];
         //      //_logger?.Debug($"Sending {8 + buf.Length} bytes");

         //      if (!_stream.IsConnected)
         //      {
         //         _logger?.Debug("Sending on disconnected chanel");
         //      }

         //      // Packet | 2 Magic | 4 Code | 8 Length | 
         //      header[0] = 0xd;
         //      header[1] = 0xe;

         //      header[2] = 0x0;
         //      header[3] = 0x0;
         //      header[4] = packetSize[0];
         //      header[5] = packetSize[1];

         //      header[6] = 0xF;

         //      header[7] = packetSize[2];
         //      header[8] = packetSize[3];
         //      header[9] = packetSize[2];
         //      header[10] = packetSize[3];

         //      await _stream.WriteAsync(header, 0, 8);
         //      await _stream.WriteAsync(buf, 0, buf.Length);
         //      //_stream.Write(header, 0, 8);
         //      //_stream.Write(buf, 0, buf.Length);

         //      //_logger?.Debug($"Sent {8 + buf.Length} bytes");
         //      //await Task.CompletedTask;
         //   }
         //}
         //finally
         //{
         //   _lock.Release();
         //}
      }

      byte [] _packetHeader = new byte[10];

      public async Task SendOpCode(int command, string payload)
      {

         try
         {
            
            _lock.Wait();
            {
               
               var buf = Encoding.UTF8.GetBytes(payload);
               byte[] packetSize = BitConverter.GetBytes(buf.Length);

               if (!_stream.IsConnected)
               {
                  _logger?.Error("Sending on disconnected");
               }

               _packetHeader[0] = 0xf;
               _packetHeader[1] = 0xe;

               _packetHeader[2] = (byte)(command / 256);
               _packetHeader[3] = (byte)(command & 255);

               _packetHeader[4] = 0xf;
               

               _packetHeader[5] = packetSize[0];
               _packetHeader[6] = packetSize[1];
               _packetHeader[7] = packetSize[2];
               _packetHeader[8] = packetSize[3];

               _packetHeader[9] = 0xf;

               
               await _stream.WriteAsync(_packetHeader, 0, _packetHeader.Length);
               await _stream.WriteAsync(buf, 0, buf.Length);
               //LogBuffer(">", command, payload);


            }
         }
         finally
         {
            _lock.Release();
         }
      }

      public async Task Send(UnitCmd msg)
      {
         if (msg == null) {
            return;
         }
         await Send(JsonConvert.SerializeObject(msg, Formatting.None));
      }

      private async Task ProcessResponse(int command, string payload)
      {
         
         try
         {
            //LogBuffer("<", command, payload);

            var message = JsonConvert.DeserializeObject<UnitCmd>(payload);            
            await Send(_processor.ProcessUnitCmd(LibCommProtoMsgType.Msg, ID, message));
         }
         catch (Exception ex)
         {
            _logger?.Error("Error in processing request, "+ ex.ToString());
         }
      }

      public Task Run(CancellationToken token)
      {
         _logger?.Info($"starting");
         return Task.Run(async () =>
         {
            try
            {
               _processor?.ProcessUnitCmd(LibCommProtoMsgType.Connected, ID, null);
               var payload = new byte[1024 * 1024];
               var header = new byte[100];               
               int packetLen = 0;
               int readData = 0;
               int transportCommand = 0;
               _logger?.Debug("Starting incomming queue");
               while (_stream.IsConnected)
               {
                  
                  readData = _stream.Read(header, 0, 10);
                  if (readData != 10) {
                     _logger?.Error("Rejecting packet");
                     continue;
                  }
                  transportCommand = BitConverter.ToUInt16(header, 2);                  
                  packetLen = BitConverter.ToInt32(header, 5);

                  readData = _stream.Read(payload, 0, payload.Length);
                  if (readData != packetLen) {
                     _logger?.Error($"Rejecting packet {transportCommand}, {packetLen}, {readData}");
                     continue;
                  }
                  
                  await ProcessResponse(transportCommand, Encoding.UTF8.GetString(payload, 0, readData));
               }
               _stream.Close();

            }
            catch (Exception ex)
            {
               _logger?.Error(ex.ToString());
            }
            finally
            {
               _processor?.ProcessUnitCmd(LibCommProtoMsgType.Disconnected, ID, null);
            }
         });
      }

      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }

      private void LogBuffer(string dir, int command, string payload)
      {
         if (string.IsNullOrEmpty(payload)) {
            return;
         }
         if (payload.Length >= 150)
         {
            _logger?.Debug($"{dir} , {command}, {payload.Substring(0, 100)}");
         }
         else {
            _logger?.Debug($"{dir} , {command}, {payload}");
         }
      }
   }

}
