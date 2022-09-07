//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================


using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using libALogger;


namespace libACommunication
{
   public class PipeClientQueued : IProtoProcessorCommand, ILDClient
   {
      private ILibALogger _logger;
      private IProtoProcessorCommand _processor;
      private NamedPipeClientStream _pipe;
      private PipeServerClientInstace _client;
      private BlockingCollection<UnitMsg> _incommingQueue = new BlockingCollection<UnitMsg>();
      private BlockingCollection<UnitMsg> _outgoingQueue = new BlockingCollection<UnitMsg>();
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

      public UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null)
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
               try
               {
                  _logger?.Info("Started incomming queue");
                  while (!token.IsCancellationRequested)
                  {
                     var data = _incommingQueue.Take(token);
                     _processor.ProcessUnitCmd(LibCommProtoMsgType.Msg, null, data);
                  }
               }
               catch
               { 
                  //catch operation cancelled.
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

      public Task Send(UnitMsg msg)
      {
         _outgoingQueue.Add(msg);
         return Task.CompletedTask;
         //return _client.Send(msg);
      }

   }
}
