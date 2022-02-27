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
using System.Net.Sockets;

namespace logAxe.http
{
   public class logAxeHttpServer : IMessageReceiver
   {
      string _serverIp;
      int _port;
      bool shouldExit = false;
      ILogger _log;
      ILogEngine _engine;
      private MessageExchangeHelper _messenger;
      private readonly string _appRootPath;

      private List<IFileObject> _lst = new List<IFileObject>();      
      Dictionary<string, WebFrame> _viewData = new Dictionary<string, WebFrame>();
      private void SendJson(HttpListenerContext ctx, string json)
      {
         var buffer = Encoding.ASCII.GetBytes(json);
         ctx.Response.ContentType = "Application/json";
         ctx.Response.ContentLength64 = buffer.Length;
         using (var writer = new BinaryWriter(ctx.Response.OutputStream))
         {
            writer.Write(buffer);
         }
      }

      Dictionary<string, byte[]> _cachedFiles = new Dictionary<string, byte[]>();
      public logAxeHttpServer(ILogger logger, ILogEngine engine, int port = 8080, string serverIp = "127.0.0.1", string appRootPath = "/logAxe/")
      {
         _appRootPath = appRootPath;
         _log = logger;
         _engine = engine;
         _port = port;
         _serverIp = serverIp;
         _messenger = new MessageExchangeHelper(_engine.MessageBroker, this);
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
               }
               break;
         }
      }

      public string GetPostData(HttpListenerContext ctx)
      {
         using (var reader = new StreamReader(ctx.Request.InputStream,
                              ctx.Request.ContentEncoding))
         {
            return reader.ReadToEnd();
         }

      }
      private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      public void Start()
      {
         _port = GetAvailablePort(_port);
         using (var shouldExitWaitHandle = new ManualResetEvent(shouldExit))
         using (var listener = new HttpListener())
         {
            Console.CancelKeyPress += (
                object sender,
                ConsoleCancelEventArgs e
            ) =>
            {
               e.Cancel = true;
               /*
               this here will eventually result in a graceful exit
               of the program
                */
               shouldExit = true;
               shouldExitWaitHandle.Set();
            };

            listener.Prefixes.Add($"http://{_serverIp}:{_port}/");
            listener.Start();
            _log.Debug($"Server started @ {_serverIp}:{_port}");
            while (!shouldExit)
            {
               try
               {

                  var contextAsyncResult = listener.BeginGetContext((IAsyncResult asyncResult) =>
                  {
                     try
                     {
                        bool sendError = true;
                        var ctx = listener.EndGetContext(asyncResult);
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
                                 writer.Write(LoadFileFileInMemory(url.Replace("/logAxe/ui/", "../../../http/data/"), true));
                              }
                           }
                           else if ("vw" == module)
                           {
                              var viewName = query["n"] == null ? "" : query["n"];
                              switch (query["op"])
                              {
                                 case "allView":
                                    SendJson(ctx, JsonConvert.SerializeObject(_viewData, Formatting.Indented));
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
                                       SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                                    }
                                    finally
                                    {
                                       _lock.Release();
                                    }

                                    break;
                                 case "info":
                                    SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
                                    break;
                                 case "ping":

                                    if (_viewData.ContainsKey(viewName))
                                    {
                                       SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
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
                                    SendJson(ctx, JsonConvert.SerializeObject(lines, Formatting.Indented));
                                    break;
                                 case "filter":
                                    if (ctx.Request.HttpMethod == "GET")
                                    {
                                       SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName].Filter, Formatting.Indented));
                                    }
                                    else
                                    {
                                       var data = GetPostData(ctx);
                                       var filter = JsonConvert.DeserializeObject<TermFilter>(data);
                                       _viewData[viewName].SetFrameAndFilter(_engine.Filter(filter), filter);
                                       SendJson(ctx, JsonConvert.SerializeObject(_viewData[viewName], Formatting.Indented));
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
                                 var process = OpenNewWindow();
                                 SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "success" } }, Formatting.Indented));
                              }
                              catch
                              {
                                 SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "failed" } }, Formatting.Indented));
                              }


                           }
                           else if ("files" == module)
                           {

                              switch (query["op"])
                              {
                                 case "addFile":
                                    var fileName = query["n"] == null ? "" : query["n"];
                                    var data = GetPostData(ctx).Split(new char[] { ',' })[1];
                                    var byteData = Convert.FromBase64String(data);
                                    _lst.Add(new WebFile(fileName, byteData));
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "status", "sucess" }, { "size", byteData.Length }, { "name", fileName } }, Formatting.Indented));
                                    break;
                                 case "process":
                                    _engine.AddFiles(_lst.ToArray(), false, true);
                                    _lst.Clear();
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                                    break;
                                 case "clear":
                                    _engine.Clear();
                                    _lst.Clear();
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                                    break;
                                 case "cancel":
                                    _lst.Clear();
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                                    break;
                                 case "status":
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
                                    break;
                                 case "lst":
                                    SendJson(ctx, JsonConvert.SerializeObject(new Dictionary<string, object>() { { "files", _engine.GetAllFileNames() } }, Formatting.Indented));
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
                                    resp.DirPaths.Add(new DirItem() { Path = @"d:\logs", Name = "Logs" });
                                    break;
                              }
                              SendJson(ctx, JsonConvert.SerializeObject(resp, Formatting.Indented));
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

                  },
                      null
                  );
                  WaitHandle.WaitAny(new WaitHandle[] { contextAsyncResult.AsyncWaitHandle, shouldExitWaitHandle });
               }
               catch (Exception ex)
               {
                  Console.Write(ex);
               }
            }
            listener.Stop();
            _log.Debug("Server stopped");
         }
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
      public Process OpenNewWindow()
      {
         var startInfo = new ProcessStartInfo();
         startInfo.FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
         startInfo.Arguments = $"--new-window --app=http://{_serverIp}:{_port}/logAxe/ui/mainUI.html";
         var process = Process.Start(startInfo);
         process.Exited += Process_Exited;
         return process;
      }
      private static void Process_Exited(object sender, EventArgs e)
      {

      }

      public int GetAvailablePort(int startPort)
      {
         do
         {
            try
            {
               var listener = new TcpListener(IPAddress.Loopback, startPort);
               listener.Start();
               listener.Stop();
               _log.Debug($"Available port found: {startPort}");
               return startPort;
            }
            catch (System.Net.Sockets.SocketException)
            {               
               _log.Debug($"occupied: {startPort}");
               startPort++;
            }
            
         } while (true);


      }
   }

   public class WebFrame
   {
      public WebFrame(string name, LogFrame logFrame)
      {
         ViewName = name;
         SetFrameAndFilter(logFrame, new TermFilter());
      }

      public void SetFrame(LogFrame logFrame)
      {
         Frame = logFrame;
         TotalLogLines = logFrame.TotalLogLines;
         SystemTotalLogLine = logFrame.SystemTotalLogLine;
         Error = logFrame.LogTypeLength(LogType.Error);
         Info = logFrame.LogTypeLength(LogType.Info);
         Warning = logFrame.LogTypeLength(LogType.Warning);
         Trace = logFrame.LogTypeLength(LogType.Trace);
         UpdateVersion();
      }

      public void SetFrameAndFilter(LogFrame logFrame, TermFilter filter)
      {
         Filter = filter;
         SetFrame(logFrame);
      }

      [JsonProperty("n")]
      public string ViewName { get; private set; }
      public int SystemTotalLogLine { get; set; }
      public int TotalLogLines { get; set; }
      public int Error { get; set; }
      public int Info { get; set; }
      public int Warning { get; set; }
      public int Trace { get; set; }
      [JsonIgnore]
      public LogFrame Frame { get; set; }
      public TermFilter Filter { get; set; }
      [JsonProperty("v")]
      public int Version { get; set; }

      public void UpdateVersion()
      {
         if (Version >= 10)
         {
            Version = 0;
         }
         else
         {
            Version += 1;
         }

      }

   }

   public class Command
   {
      [JsonProperty("op")]
      public string Operation { get; set; }
      public string[] Paths { get; set; }
   }

   public class FileBrowserResponse
   {

      [JsonProperty("op")]
      public string Operation { get; set; }
      [JsonProperty("files")]
      public List<DirItem> FilePaths { get; set; } = new List<DirItem>();

      [JsonProperty("dirs")]
      public List<DirItem> DirPaths { get; set; } = new List<DirItem>();
   }

   public class DirItem
   {
      [JsonProperty("p")]
      public string Path { get; set; }
      [JsonProperty("n")]
      public string Name { get; set; }
   }

   public class WebLogLines
   {
      [JsonProperty("n")]
      public string ViewName { get; }
      public List<LogLine> LogLines { get; }
      public WebLogLines(string name, int capacity)
      {
         ViewName = name;
         LogLines = new List<LogLine>(capacity);
      }
   }
   public class UploadedFile
   {
      public string Name { get; set; }
      public int Size { get; set; }
      public string Data { get; set; }
   }

   class ViewData
   {
      public LogFrame Frame { get; set; } = null;
      public TermFilter Filter { get; set; } = new TermFilter();
   }
}