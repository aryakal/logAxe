//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using libALogger;


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
         switch (msgType)
         {
            case LibCommProtoMsgType.Connected:
               _logger?.Debug($"{msgType}, {clientInfo.UniqueId}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
               _processor.TotalClients(_pDHelper.Clients.Count);
               return null;

            case LibCommProtoMsgType.Disconnected:
               _pDHelper.RemoveClient(clientInfo);
               _logger?.Debug($"{msgType}, {clientInfo.UniqueId}, Clients {_pDHelper.TotalClients}, dict {_pDHelper.Clients.Count}");
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
               try
               {
                  var server = new NamedPipeServerStream(_pipeName, PipeDirection.InOut, 254, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                  server.WaitForConnectionAsync(token).Wait();
                  _logger?.Debug("Client connected");
                  var id = SimpleClientInfogGenerator.Generate();
                  var client = new PipeServerClientInstace(Logging.GetLogger(id.UniqueId), id, server, this);
                  _pDHelper.AddClient(client.ID, client);
                  _ = client.Run(token);
               }
               catch (TaskCanceledException)
               {
               }
               catch (AggregateException)
               {
               }
            }

            _logger?.Debug("RunForever, wait over.");
         });
      }
      public void RunForever(CancellationToken token)
      {
         Run(token).Wait();
      }
      public Task Send(IClientInfo id, UnitMsg msg)
      {
         return _pDHelper.Clients[id.UniqueId].Send(msg);
      }
   }
}
