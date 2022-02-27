//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Text;

using Newtonsoft.Json;
using logAxeCommon;
using logAxeEngine.Interfaces;
using logAxeEngine.Common;


using libWebServer;


namespace logAxe.http
{
   public class logAxeHttpServerProxy : IMessageReceiver
   {
      private ILogger _log;
      private ILogEngine _engine;
      private MessageExchangeHelper _messenger;
      private readonly string _appRootPath;
      private List<IFileObject> _lst = new List<IFileObject>();
      Dictionary<string, byte[]> _cachedFiles = new Dictionary<string, byte[]>();
      ILibWebServer _libWebServer;
      private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      class ViewData
      {
         public LogFrame Frame { get; set; } = null;
         public TermFilter Filter { get; set; } = new TermFilter();
      }
      Dictionary<string, WebFrame> _viewData = new Dictionary<string, WebFrame>();

      class WebLogProxy : ILibWebServerLogger
      {
         ILogger _logger;
         public WebLogProxy(ILogger logger)
         { 
             _logger = logger;
         }
         public void Debug(string message)
         {
            _logger.Debug(message);
         }

         public void Error(string message)
         {
            _logger.Error(message);
         }

         public void Trace(string message)
         {
            _logger.Debug(message);
         }
      }

      public logAxeHttpServerProxy(ILogger logger, ILogEngine engine, int port = 8080, string serverIp = "127.0.0.1", string appRootPath = "/logAxe/", bool debugHttp=false)
      {
         _appRootPath = appRootPath;
         _log = logger;
         _engine = engine;
         _messenger = new MessageExchangeHelper(_engine.MessageBroker, this);
         _libWebServer = new LibWebServer(
             address: serverIp, 
             port: port, 
             logger: debugHttp ? new WebLogProxy(_log) : null);
         //_libWebServer = new LibWebServer(serverIp, port);
         _libWebServer.HttpMessage = ProcessWebRequest;
         _libWebServer.WsMessage = ProcessWebSocketRequest;
      }

      public void GetMessage(ILogAxeMessage message)
      {
         switch (message.MessageType)
         {
            case LogAxeMessageEnum.NewViewAnnouncement:
               {
                  foreach (var v in _viewData)
                  {
                     var view = v.Value;
                     view.SetFrame(_engine.Filter(view.Filter));
                  }
                  _libWebServer.BroadcastMsg(JsonConvert.SerializeObject(new WsOperation("refreshView", "all"), Formatting.Indented));
               }
               break;
         }
      }

      public void Start()
      {
         _libWebServer.RunForever();
      }

      //private void ServerFile(HttpListenerContext ctx, string filePath)
      //{
      //   var response = ctx.Response;
      //   using (FileStream fs = File.OpenRead(filePath))
      //   {
      //      string filename = Path.GetFileName(filePath);            
      //      response.ContentLength64 = fs.Length;
      //      response.SendChunked = false;
      //      using (var writer = new BinaryWriter(ctx.Response.OutputStream))
      //      {
      //         writer.Write();
      //      }
      //      //response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
      //      //response.AddHeader("Content-disposition", "attachment; filename=" + filename);
      //   }
      //}
      private byte[] LoadFileFileInMemory(string filePath, bool recatch = false)
      {
         if (!_cachedFiles.ContainsKey(filePath) || recatch)
         {
            _cachedFiles[filePath] = File.ReadAllBytes(filePath);
         }
         return _cachedFiles[filePath];
      }
      public void StartWebBrowser()
      {
         _libWebServer.StartWebBrowser();         
      }
      private static void Process_Exited(object sender, EventArgs e)
      {

      }

      public void ProcessWebRequest(HttpListenerContext ctx)
      {
         try
         {
            bool sendError = true;
            _log.Debug($"url, {ctx.Request.RawUrl}");

            var url = ctx.Request.Url.AbsolutePath;
            if (url == "/")
            {
               ctx.Response.Redirect($"{_appRootPath}ui/mainUI.html");
               ctx.Response.StatusCode = 302;
               sendError = false;
            }
            else if (url.StartsWith(_appRootPath))
            {
               sendError = false;
               var words = url.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
               var appRoot = words.Length > 0 ? words[0] : "logAxe";
               var module = words.Length > 1 ? words[1] : "";

               var query = ctx.Request.QueryString;

               if ("ui" == module)
               {

                  using (var writer = new BinaryWriter(ctx.Response.OutputStream))
                  {
                     writer.Write(LoadFileFileInMemory(url.Replace("/logAxe/ui/", "http/data/"), true));
                  }
               }
               else if ("vw" == module)
               {
                  var viewName = query["n"] == null ? "" : query["n"];
                  switch (query["op"])
                  {
                     case "allView":
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData, Formatting.Indented));
                        break;
                     case "createView":
                        try
                        {
                           _lock.Wait();
                           for (var ndx = 1; ndx < 999; ndx++)
                           {
                              viewName = $"v{ndx}";
                              if (!_viewData.ContainsKey(viewName))
                              {
                                 _viewData[viewName] = new WebFrame(viewName, _engine.GetMasterFrame());
                                 break;
                              }
                           }//TODO clear the old views.
                           WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                        }
                        finally
                        {
                           _lock.Release();
                        }

                        break;
                     case "info":
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                        break;
                     case "ping":

                        if (_viewData.ContainsKey(viewName))
                        {
                           WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                        }
                        else
                        {
                           ctx.Response.StatusCode = 410;
                           using (var writer = new StreamWriter(ctx.Response.OutputStream))
                           {
                              writer.Write("cannot find the view name");
                           }
                        }
                        //ctx.Response.StatusCode = 410;
                        //using (var writer = new StreamWriter(ctx.Response.OutputStream))
                        //{
                        //   writer.Write("cannot find the view name");
                        //}

                        break;
                     case "line":
                        var startLine = int.Parse(query["s"]);
                        var length = int.Parse(query["l"]);
                        var lines = new WebLogLines(viewName, length);
                        for (int ndx = startLine; ndx < length + startLine; ndx++)
                        {
                           if (ndx >= _viewData[viewName].TotalLogLines)
                           {
                              break;
                           }
                           lines.LogLines.Add(_engine.GetLogLine(_viewData[viewName].Frame.TranslateLine(ndx)));
                        }
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(lines, Formatting.Indented));
                        break;
                     case "filter":
                        if (ctx.Request.HttpMethod == "GET")
                        {
                           WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName].Filter, Formatting.Indented));
                        }
                        else
                        {
                           var data = WebHelper.GetPostData(ctx);
                           var filter = JsonConvert.DeserializeObject<TermFilter>(data);
                           _viewData[viewName].SetFrameAndFilter(_engine.Filter(filter), filter);
                           WebHelper.SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                        }
                        break;
                     case null:
                        break;
                  }
               }
               else if ("openView" == module)
               {
                  try
                  {
                     StartWebBrowser();
                     WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "success" } }, Formatting.Indented));
                  }
                  catch
                  {
                     WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "failed" } }, Formatting.Indented));
                  }


               }
               else if ("files" == module)
               {

                  switch (query["op"])
                  {
                     case "addFile":
                        var fileName = query["n"] == null ? "" : query["n"];
                        var data = WebHelper.GetPostData(ctx).Split(new char[] { ',' })[1];
                        var byteData = Convert.FromBase64String(data);
                        _lst.Add(new WebFile(fileName, byteData));
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "sucess" }, { "size", byteData.Length }, { "name", fileName } }, Formatting.Indented));
                        break;
                     case "process":
                        _engine.AddFiles(_lst.ToArray(), false, true);
                        _lst.Clear();
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                        break;
                     case "clear":
                        _engine.Clear();
                        _lst.Clear();
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                        break;
                     case "cancel":
                        _lst.Clear();
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                        break;
                     case "status":
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                        break;
                     case "lst":
                        WebHelper.SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                        break;
                  }


               }
               else if ("fileBrowser" == module)
               {
                  var op = query["op"];
                  var resp = new FileBrowserResponse() { Operation = op };
                  switch (op)
                  {
                     case "lst":
                        var path = query["path"];
                        if (path != null && Directory.Exists(path))
                        {
                           foreach (var filePath in Directory.GetFiles(path))
                           {
                              resp.FilePaths.Add(new DirItem() { Path = filePath, Name = Path.GetFileName(filePath) });
                           }

                           foreach (var dirPath in Directory.GetDirectories(path))
                           {
                              resp.DirPaths.Add(new DirItem() { Path = dirPath, Name = Path.GetFileName(dirPath) });
                           }

                        }
                        break;

                     case "fav":
                        // TODO : make this a generic code or an input from json !
                        resp.DirPaths.Add(new DirItem() { Path = @"c:\", Name = "c Drive" });
                        resp.DirPaths.Add(new DirItem() { Path = @"d:\", Name = "d Drive" });                        
                        break;
                  }
                  WebHelper.SendJson(ctx, JsonConvert.SerializeObject(resp, Formatting.Indented));
               }
               else
               {
                  sendError = true;
               }
            }
            if (sendError)
            {
               ctx.Response.StatusCode = 404;
               using (var writer = new StreamWriter(ctx.Response.OutputStream))
               {
                  writer.Write("cannot find the file.");
               }
            }

         }
         catch (Exception ex)
         {
            Console.Write(ex);
         }
      }
      public void ProcessWebSocketRequest(WebSocketMsgType msgType, ISocketID clientInfo, string msg)
      {


         try
         {
            if (string.IsNullOrEmpty(msg)) { return; }
            var d = JsonConvert.DeserializeObject<WsOperation>(msg);
            _log.Debug($"Ws | {msgType}, {clientInfo.ID}-> {d.OpCode} | {d.Name} | {msg}");
            switch (d.OpCode)
            {
               case "lines":
                  {
                     var t = typeof(CmdLines).ToString();
                     var l1 = JsonConvert.DeserializeObject<CmdLines>(d.Value.ToString());

                     var lines = new WebLogLines(d.Name, l1.Length);
                     for (int ndx = l1.StartLine; ndx < l1.Length + l1.StartLine; ndx++)
                     {
                        if (ndx >= _viewData[d.Name].TotalLogLines)
                        {
                           break;
                        }
                        lines.LogLines.Add(_engine.GetLogLine(_viewData[d.Name].Frame.TranslateLine(ndx)));
                     }

                     _libWebServer.SendMsg(clientInfo, JsonConvert.SerializeObject(new WsOperation(d.OpCode, d.Name, lines), Formatting.Indented));

                  }
                  break;
               case "getInfo":
                  {

                     if (d.Name == "")
                     {
                        try
                        {
                           _lock.Wait();
                           for (var ndx = 1; ndx < 999; ndx++)
                           {
                              d.Name = $"v{ndx:03d}";
                              if (!_viewData.ContainsKey(d.Name))
                              {
                                 _viewData[d.Name] = new WebFrame(d.Name, _engine.GetMasterFrame());
                                 break;
                              }
                           }//TODO clear the old views.

                        }
                        finally
                        {
                           _lock.Release();
                        }
                     }
                     _libWebServer.SendMsg(clientInfo, JsonConvert.SerializeObject(new WsOperation(d.OpCode, d.Name, _viewData[d.Name]), Formatting.Indented));
                  }
                  break;
               case "clearFiles":
                  {
                     _engine.Clear();
                  }
                  break;
               case "filter":
                  {
                     var filter = JsonConvert.DeserializeObject<TermFilter>(d.Value.ToString());
                     _viewData[d.Name].SetFrameAndFilter(_engine.Filter(filter), filter);
                     _libWebServer.SendMsg(clientInfo, JsonConvert.SerializeObject(new WsOperation("getInfo", d.Name, _viewData[d.Name]), Formatting.Indented));
                  }
                  break;
            }
         }
         catch (Exception ex)
         {

            _log.Error($"{msgType}, {clientInfo.ID}-> {msg}");
            _log.Error(ex.ToString());
         }
      }

      class WsOperation
      {
         public WsOperation() { }

         public WsOperation(string opCode, string name, object value=null)
         {
            OpCode = opCode;
            Name = name;            
            Value = (null != value)? value: new Dictionary<string, string>();
         }

         [JsonProperty("op")]
         public string OpCode { get; set; }
         [JsonProperty("name")]
         public string Name { get; set; }
         [JsonProperty("value")]
         public object Value { get; set; }
      }

      class CmdLines
      {
         [JsonProperty("s")]
         public int StartLine { get; set; }
         [JsonProperty("l")]
         public int Length { get; set; }
         
      }

      
   }
}