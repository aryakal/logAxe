//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Net;
using System.Threading.Tasks;

namespace libWebServer
{
   public interface ILibWebServer
   {
      Task Run();
      void RunForever();
      void Stop();
      void BroadcastMsg(string msg);
      void SendMsg(ISocketID info, string msg);
      Action<HttpListenerContext> HttpMessage { get; set; }
      Action<WebSocketMsgType, ISocketID, string> WsMessage { get; set; }

      void StartWebBrowser();

   }


   public interface ILibWebServerLogger { 
      void Trace(string message);
      void Debug(string message);
      void Error(string message);
   }

  
   
   public class SimpleLogger : ILibWebServerLogger
   {
      public void Trace(string message)
      {
         Console.WriteLine("T |  " + message);
      }
      public void Debug(string message)
      {
         Console.WriteLine("D | " + message);
      }
      public void Error(string message)
      {
         Console.WriteLine("E | " + message);
      }
   }

   public class NullLogger : ILibWebServerLogger
   {
      public void Debug(string message)
      {
        
      }

      public void Error(string message)
      {
         
      }

      public void Trace(string message)
      {
         
      }
   }

}