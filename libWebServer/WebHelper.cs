//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Net;
using System.Text;
using System.IO;

namespace libWebServer
{
   public static class WebHelper
   {
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

      public static string GetPostData(HttpListenerContext ctx)
      {
         using (var reader = new StreamReader(ctx.Request.InputStream,
                              ctx.Request.ContentEncoding))
         {
            return reader.ReadToEnd();
         }

      }
   }
}
