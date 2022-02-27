//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using logAxeEngine.Interfaces;
using logAxeEngine.Common;

namespace logAxeEngine.Engines
{
   public class LogMessageEngine : IMessageBroker
   {
      private Task _eventQueue;
      private CancellationTokenSource _cancelBackgroundTask;
      private SemaphoreSlim _lockMessageQueue = new SemaphoreSlim(1, 1);
      private Dictionary<string, IMessageReceiver> _messageClients = new Dictionary<string, IMessageReceiver>();
      private BlockingCollection<ILogAxeMessage> _msgQueue = new BlockingCollection<ILogAxeMessage>();
      private NamedLogger _logger = new NamedLogger("msgEngine");
      public LogMessageEngine()
      {
      }
      public void SendMessage(ILogAxeMessage message)
      {
         _msgQueue.Add(message);
      }
      public string RegisterClient(IMessageReceiver reciver)
      {
         try
         {
            _lockMessageQueue.Wait();
            var uid = $"client-{DateTime.Now:yyyyMMddHHmmssfff}";
            _messageClients.Add(uid, reciver);
            return uid;
         }
         catch
         {
            _logger.Error($"Error in registering client.");
            throw;
         }
         finally
         {
            _lockMessageQueue.Release();
         }
      }
      public void UnregisterClient(string clientId)
      {
         try
         {
            _lockMessageQueue.Wait();
            if (_messageClients.ContainsKey(clientId))
            {
               _ = _messageClients.Remove(clientId);
            }

         }
         catch
         {
            _logger.Error($"Error in unregister client.");
         }
         finally
         {
            _lockMessageQueue.Release();
         }
      }
      public void Start()
      {
         _logger.Debug("Starting message engine");
         _cancelBackgroundTask = new CancellationTokenSource();
         _eventQueue = Task.Factory.StartNew(() =>
         {
            MessageQueuePump();
         }, TaskCreationOptions.LongRunning);
      }
      public void Stop()
      {
         _cancelBackgroundTask.Cancel();
         _eventQueue.Wait();
         _logger.Debug("LogMessageEngine, Stop, Stop");
      }
      private void MessageQueuePump()
      {
         while (!_cancelBackgroundTask.IsCancellationRequested)
         {
            try
            {
               var message = _msgQueue.Take(_cancelBackgroundTask.Token);

               try
               {
                  _lockMessageQueue.Wait(_cancelBackgroundTask.Token);
                  foreach (var client in _messageClients)
                  {
                     if (client.Key != message.FromClientID)
                     {
                        try
                        {
                           client.Value?.GetMessage(message);
                        }
                        catch
                        {
                           _logger.Error($"Client {client.Key} has error");
                        }
                     }
                  }
               }
               finally
               {
                  _lockMessageQueue.Release();
               }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
               _logger.Error($"Error detected in message queue. {ex}");
            }
         }
      }

   }
}
