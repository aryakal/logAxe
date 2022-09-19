//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using libACommunication;
using libALogger;
using logAxeCommon;

namespace logAxe
{
   public class Communication : IProtoProcessorCommand
   {
      internal class LogAxeClient 
      {
         public string UniqueId { get; set; }
         public bool IsViewRequired { get; set; }
         public Action<UnitMsg> Callback { get; set; }
      }
      ILibALogger _logger;
      ILDClient _client;
      Task _backgroundClientServer;
      CancellationTokenSource _cts;
      Dictionary<string, LogAxeClient> _clients = new Dictionary<string, LogAxeClient>();
      SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      public Communication(string serverName)
      {
         _logger = null;// Logging.GetLogger("comm");
         _cts = new CancellationTokenSource();

         _client = new PipeClientQueued(
             logger: null,// Logging.GetLogger("cli"),
             processor: this,
             serverName
             );
      }

      public void Connect()
      {
         _logger?.Debug("Connecting to the server");
         Reconnect();
         //_backgroundClientServer = _client.Run(_cts.Token);
         //_client.Send(new UnitCmd("test1", "", null));
         //_backgroundClientServer.Wait();


         //var cts = new CancellationTokenSource();
         //var client = new PipeClient(
         //    logger: Logging.GetLogger("cli"),
         //   processor: new ClientTest());
         //var t = client.Run(cts.Token);

         //client.Send(new UnitCmd("test1", "", null));
         ////client.RunForever(cts.Token);
         //t.Wait();

      }
      public void Diconnect()
      {
         _cts.Cancel();
         _backgroundClientServer.Wait();
      }

      public UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null)
      {
         //TODO : Still donot know what will happen to disconnect.
         switch (msgType)
         {
            case LibCommProtoMsgType.Msg:
               _logger?.Debug($"{message.UniqueId}, {message.OpCode}");
               if (message.UniqueId == WebFrameWork.CLIENT_BST_ALL)
               {
                  foreach (var client in _clients) {
                     
                     _clients[client.Key].Callback(message);
                  }
               }
               else {
                  _clients[message.UniqueId].Callback(message);
               }
               
               
               break;
            case LibCommProtoMsgType.Disconnected:
               _logger?.Debug($"disconnected");
               Reconnect();
               break;
         }
         return null;

      }

      public void RegisterClient(string clientName, Action<UnitMsg> command, bool isViewRequired)
      {
         try
         {
            _lock.Wait();
            _logger?.Info($"adding client {clientName}");
            var client = new LogAxeClient() { UniqueId = clientName, IsViewRequired = isViewRequired, Callback = command };
            _clients[client.UniqueId] = client;
         }
         finally {
            _lock.Release();
         }
         RegisterAllClients();
      }
      public void UnRegisterClient(string clientName) {
         try
         {
            _lock.Wait();
            _logger?.Info($"removing client {clientName}");
            if (_clients.ContainsKey(clientName))
            {
               _clients.Remove(clientName);
               SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_UNREGISTER, name: clientName,
                    value: new RegisterClient()
                    {
                       Name = clientName                       
                    }
                    )); ;
            }

         }
         finally
         {
            _lock.Release();
         }
      }

      public void SendMsg(UnitMsg cmd)
      {
         _client.Send(cmd).Wait();
      }

      private void RegisterAllClients() {
         try
         {            
            _lock.Wait();
            _logger?.Debug($"Registering {_clients.Count} clients");
            if (_clients.Count > 0)
            {
               foreach (var client in _clients)
               {
                  SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_REGISTER, name: client.Key,
                     value: new RegisterClient()
                     {
                        Name = client.Key,
                        IsViewRequired = client.Value.IsViewRequired
                     }
                     )); ;
               }
            }
         }
         finally
         {
            _lock.Release();
         }
      }
      private Task Reconnect() {
         return Task.Run(() =>
         {
            while (true)
            {
               try
               {
                  _logger?.Debug("Connecting to the server");
                  _backgroundClientServer = _client.Run(_cts.Token);                  
                  RegisterAllClients();
                  break;
               }
               catch 
               { 
               }
            }
         });
      }
   }
}
