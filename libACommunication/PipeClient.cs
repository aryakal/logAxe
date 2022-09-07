//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using libALogger;


namespace libACommunication
{
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

      public UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null)
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

      public Task Send(UnitMsg msg)
      {
         return _client.Send(msg);
      }

   }
}
