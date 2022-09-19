//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace libACommunication
{
   public enum LibCommProtoMsgType
   {
      Connected,
      Disconnected,
      Msg,
      ClientId
   }

   public class UnitMsg
   {
      public UnitMsg() { }
      public UnitMsg(string opCode, string name, object value = null, string responseStatus = "None")
      {
         OpCode = opCode;
         UniqueId = name;
         Value = (null != value) ? value : new Dictionary<string, string>();
         Status = responseStatus;
      }

      public UnitMsg(string opCode, IClientInfo info, object value = null, string responseStatus = "None")
      {
         OpCode = opCode;
         UniqueId = info?.UniqueId;
         Value = (null != value) ? value : new Dictionary<string, string>();
         Status = responseStatus;
      }


      public string OpCode { get; set; }
      public string UniqueId { get; set; }
      public object Value { get; set; }
      public string Status { get; set; }

      public T GetData<T>()
      {
         return JsonConvert.DeserializeObject<T>(Value.ToString());
      }
   }
   public interface IClientInfo
   {
      string UniqueId { get; }
   }

   public class SimpleClientInfo : IClientInfo
   {
      public SimpleClientInfo(string id)
      {
         UniqueId = id;
      }
      public string UniqueId { get; }
   }

   public static class SimpleClientInfogGenerator
   {
      private static int _socketClientId;
      public static SimpleClientInfo Generate()
      {
         int socketId = Interlocked.Increment(ref _socketClientId);
         return new SimpleClientInfo($"c{socketId}");
      }
   }

   public interface IProtoProcessorProcessClientsAdv<T> : IProtoProcessorProcessClients
   {
      void ProcessUnitCmd(T context);

   }

   public interface IProtoProcessorProcessClients : IProtoProcessorCommand
   {
      void TotalClients(long noOfClients);
   }

   /// <summary>
   /// Any client which wants to process LibCommProtoMsgType and give a respone. 
   /// </summary>
   public interface IProtoProcessorCommand
   {
      UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null);
   }

   public interface ILDClient
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(UnitMsg msg);

   }

   public interface IDLServer : IProtoProcessorCommand
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(IClientInfo id, UnitMsg msg);
      Task BroadCast(UnitMsg msg);
   }

   public interface IPLCommunication
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(string msg);
      Task Send(UnitMsg msg);
      IClientInfo ID { get; }
   }

   public class PDHelper<T>
   {

      public Dictionary<string, T> Clients = new Dictionary<string, T>();
      long _totalClients = 0;
      public long AddClient(IClientInfo clientInfo, T instance)
      {
         Clients[clientInfo.UniqueId] = instance;
         return Interlocked.Increment(ref _totalClients);
      }

      public long RemoveClient(IClientInfo clientInfo)
      {
         if (Clients.ContainsKey(clientInfo.UniqueId))
         {
            Clients.Remove(clientInfo.UniqueId);
            return Interlocked.Decrement(ref _totalClients);
         }
         return TotalClients;
      }

      public long TotalClients
      {
         get
         {
            return Interlocked.Read(ref _totalClients);
         }
      }
   }
}
