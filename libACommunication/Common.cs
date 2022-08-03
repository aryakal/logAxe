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

   public class UnitCmd
   {
      public UnitCmd() { }
      public UnitCmd(string opCode, string name, object value = null, string responseStatus = "None")
      {
         OpCode = opCode;
         UID = name;
         Value = (null != value) ? value : new Dictionary<string, string>();
         Status = responseStatus;
      }

      public UnitCmd(string opCode, IClientInfo info, object value = null, string responseStatus = "None")
      {
         OpCode = opCode;         
         UID = info?.ID;
         Value = (null != value) ? value : new Dictionary<string, string>();
         Status = responseStatus;
      }

 
      public string OpCode { get; set; }
      public string UID { get; set; }      
      public object Value { get; set; }
      public string Status { get; set; } = null;

      public T GetData<T>() { 
         return JsonConvert.DeserializeObject<T>(Value.ToString());
      }
   }
   public interface IClientInfo
   {
      string ID { get; }
   }

   public class SimpleClientInfo : IClientInfo
   {
      public SimpleClientInfo(string id)
      {
         ID = id;
      }
      public string ID { get; }
   }

   public static class SimpleClientInfogGenarator
   {
      private static int _socketClientId = 0;
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
      UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null);
   }

   public interface ILDClient
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(UnitCmd msg);

   }

   public interface IDLServer : IProtoProcessorCommand
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(IClientInfo id, UnitCmd msg);
      Task BroadCast(UnitCmd msg);
   }

   public interface IPLCommunication
   {
      Task Run(CancellationToken token);
      void RunForever(CancellationToken token);
      Task Send(string msg);
      Task Send(UnitCmd msg);
      IClientInfo ID { get; }
   }

   public class PDHelper<T>
   {

      public Dictionary<string, T> Clients = new Dictionary<string, T>();
      long _totalClients = 0;
      public long AddClient(IClientInfo clientInfo, T instance)
      {
         Clients[clientInfo.ID] = instance;
         return Interlocked.Increment(ref _totalClients);
      }

      public long RemoveClient(IClientInfo clientInfo)
      {
         if (Clients.ContainsKey(clientInfo.ID))
         {
            Clients.Remove(clientInfo.ID);
            return Interlocked.Decrement(ref _totalClients);
         }
         return TotalClients;
      }

      public long TotalClients { get {
            return Interlocked.Read(ref _totalClients);
         } 
      }

      //public interface ICommandCovertor {
      //   object ConvertFromPayload(int opCommand, string data);
      //   string ConvertToPayload(int opCommand, object data);
      //}
   }
}
