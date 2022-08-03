//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using logAxeCommon;
using logAxeCommon.Files;
using logAxeCommon.Interfaces;
using logAxeEngine.Interfaces;
using logAxeEngine.Common;

using libACommunication;
using libALogger;



namespace logAxe.http
{
   public class logAxeHttpServerProxy : IMessageReceiver, IProtoProcessorProcessClientsAdv<HttpListenerContext>
   {
      private ILibALogger _log;
      private ILogEngine _engine;
      private MessageExchangeHelper _messenger;
      private readonly string _appRootPath;
      private List<IFileObject> _lst = new List<IFileObject>();
      Dictionary<string, byte[]> _cachedFiles = new Dictionary<string, byte[]>();
      IDLServer _libWebServer;
      private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      private bool _exitOnLastClient =  false;
      // private string _internalPath = "./data/";
      private string _internalPath = "../../http/data/";
      class ViewData
      {
         public LogFrame Frame { get; set; } = null;
         public TermFilter Filter { get; set; } = new TermFilter();
      }
      Dictionary<string, WebFrame> _viewData = new Dictionary<string, WebFrame>();
      public logAxeHttpServerProxy(ILibALogger logger, ILogEngine engine, int port = 8080, string serverIp = "127.0.0.1", string appRootPath = "/logAxe/", bool debugHttp = false)
      {
         _appRootPath = appRootPath;
         _log = logger;
         _engine = engine;
         //_messenger = new MessageExchangeHelper(_engine.MessageBroker, this);
         _libWebServer = new LibWebServer(
            processor: this,
            address: serverIp,
            port: port,
            logger: debugHttp ? Logging.GetLogger("http") : null,
            websoketLogger: debugHttp ? Logging.GetLogger("ws") : null
            );

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
                  _libWebServer.BroadCast(new UnitCmd("refreshView", "all", responseStatus: WebHelper.RespSuccess));
               }
               break;
         }
      }

      public void Start()
      {
         CancellationTokenSource cts = new CancellationTokenSource(); //TODO remove from here.
         var tsk = Task.Factory.StartNew(() =>
         {
            _log?.Info($"starting background tsk");
            _log?.Info($"web root,  { Path.GetFullPath(_internalPath)}");
            while (true)
            {
               try
               {
                  if (null != _log)
                  {
                     //_log.Debug($"total views {0} ");
                  }
                  Task.Delay(2000).Wait();

               }
               catch (Exception ex)
               {
                  Console.Write(ex);
               }
            }
         });
         _libWebServer.RunForever(cts.Token);
      }

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
         _log?.Info("Starting web server");
         //_libWebServer.StartWebBrowser(); //TODO : web , need interface helper functions to launch the web browser.
      }

      public void ProcessHttpRequest(HttpListenerContext ctx)
      {
         try
         {

            bool sendError = true;
            //_log.Debug($"url, {ctx.Request.RawUrl}");

            var url = ctx.Request.Url.AbsolutePath;
            if (url == "/")
            {
               ctx.Response.Redirect($"{_appRootPath}ui/mainUI.html");
               ctx.Response.Close();
               //ctx.Response.StatusCode = 302;
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
                     var absPath = Path.GetFullPath(url.Replace("/logAxe/ui/", _internalPath));
                     //TODO security
                     //_logger.Debug($"loading, {absPath}");
                     //TODO reaload when there is debug 
                     if (File.Exists(absPath))
                     {
                        writer.Write(LoadFileFileInMemory(absPath, true));
                     }
                     else
                     {
                        _log.Error($"Not found : {absPath}");
                        sendError = true;
                     }

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
                        WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "status", "sucess" }, { "size", byteData.Length }, { "name", fileName } }));
                        break;
                        //case "process":
                        //   _engine.AddFiles(_lst.ToArray(), false, true);
                        //   _lst.Clear();
                        //   WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "files", _engine.GetAllLogFileInfo() } }));
                        //   break;
                        //case "clear":
                        //   _engine.Clear();
                        //   _lst.Clear();
                        //   WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "files", _engine.GetAllLogFileInfo() } }));
                        //   break;
                        //case "cancel":
                        //   _lst.Clear();
                        //   WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "files", _engine.GetAllLogFileInfo() } }));
                        //   break;
                        //case "status":
                        //   WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "files", _engine.GetAllLogFileInfo() } }));
                        //   break;
                        //case "lst":
                        //   WebHelper.SendJson(ctx, Utils.ToJson(new Dictionary<string, object>() { { "files", _engine.GetAllLogFileInfo() } }));
                        //   break;


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
                  WebHelper.SendJson(ctx, Utils.ToJson(resp));
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

      public UnitCmd ProcessWsOperation(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {
         try
         {
            if (msgType != LibCommProtoMsgType.Msg)
               return null;
            switch (message.OpCode)
            {
               case "lines":
                  {
                     if (message.UID == null)
                        return null;
                     var l1 = Utils.FromJson<CmdLines>(message.Value.ToString());
                     var lines = new WebLogLines(message.UID, l1.Length);
                     for (int ndx = l1.StartLine; ndx < l1.Length + l1.StartLine; ndx++)
                     {
                        if (ndx >= _viewData[message.UID].TotalLogLines)
                        {
                           break;
                        }
                        lines.LogLines.Add(_engine.GetLogLine(_viewData[message.UID].Frame.TranslateLine(ndx)));
                     }
                     return new UnitCmd(message.OpCode, message.UID, lines, responseStatus: WebHelper.RespSuccess);
                  }
               case "getInfo":
                  {
                     if (!_viewData.ContainsKey(clientInfo.ID) || clientInfo.ID != message.UID)
                     {
                        message.UID = clientInfo.ID;
                        _viewData[clientInfo.ID] = new WebFrame(clientInfo.ID, _engine.GetMasterFrame());
                     }
                     return new UnitCmd(message.OpCode, message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess);
                  }

               case "clearFiles":
                  {
                     _engine.Clear();
                  }
                  break;
               case "filter":
                  {
                     var filter = Utils.FromJson<TermFilter>(message.Value.ToString());
                     _viewData[message.UID].SetFrameAndFilter(_engine.Filter(filter), filter);
                     return new UnitCmd("getInfo", message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess);
                  }
               case "config":
                  {
                     var config = Utils.ToJson<ConfigUI>(new ConfigUI());
                     Utils.ToJson(new UnitCmd("getInfo", message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess));
                     break;
                  }
               case "openView":
                  {
                     StartWebBrowser();
                     break;
                  }
               case "process":
                  {                     
                     _engine.AddFiles(_lst.ToArray(), false, true);
                     _lst.Clear();
                     break;
                  }
               default:
                  //message.Value = null;
                  //message.Status = WebHelper.RespFailed;
                  SetFailed(message, $"{message.OpCode}, Command does not exists.");
                  return message;

            }

         }
         catch (Exception ex)
         {
            _log.Error($"{msgType}, {clientInfo.ID}");
            _log.Error(ex.ToString());
            message.Value = null;
            message.Status = WebHelper.RespFailed;
            return message;
         }

         message.Value = null;
         message.Status = WebHelper.RespSuccess;
         return message;
      }
      private void SetFailed(UnitCmd message, string reason)
      {
         message.Value = new Dictionary<string, object>() { { "reason", reason } };
         message.Status = WebHelper.RespFailed;
      }

      public void ProcessTotalClients(int totalClients)
      {
         if (totalClients == 0)
         {
            _log?.Info($"Detected no clients connected, signal to close. exitOnLastClient {_exitOnLastClient}");
            //TODO : enable the ability to close a server.
            //if(_exitOnLastClient)
            //   _libWebServer.Stop();
         }
      }

      public static void Test() {
         //var client = new WebSocketClientTest(
         //   bufferSize:4096, 
         //   logger: new WebLogProxy(new NamedLogger("ws")));
         //client.ProcessWebSocket().Wait();
      }

      public void ProcessUnitCmd(HttpListenerContext context)
      {
         throw new NotImplementedException();
      }

      public void TotalClients(long noOfClients)
      {
         throw new NotImplementedException();
      }

      public UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {
         throw new NotImplementedException();
      }
   }


   public class logAxePipeServer : IMessageReceiver, IProtoProcessorProcessClients, IMessageExchanger
   {
      private ILibALogger _logger;
      private ILogEngine _engine;
      private MessageExchangeHelper _messenger;
      private readonly string _appRootPath;
      CommonFunctionality _commonFunctionality;
      private List<IFileObject> _lst = new List<IFileObject>();
      Dictionary<string, byte[]> _cachedFiles = new Dictionary<string, byte[]>();
      //Dictionary<string, >
      IDLServer _libPipeServer;
      private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      private bool _exitOnLastClient = false;      
      class ViewData
      {
         public LogFrame Frame { get; set; } = null;
         public TermFilter Filter { get; set; } = new TermFilter();
      }
      Dictionary<string, WebFrame> _viewData = new Dictionary<string, WebFrame>();
      public logAxePipeServer(ILibALogger logger, ILogEngine engine, string pipeName, CommonFunctionality commonFunctionality)
      {
         _logger = logger;
         _commonFunctionality = commonFunctionality;
         _engine = engine;
         _engine.RegisterMessageExchanger(this);
         // _messenger = new MessageExchangeHelper(_engine.MessageBroker, this);
         _libPipeServer = new PipeServer(
            logger: Logging.GetLogger("srv"),
            processor: this,
            pipeName
            ); 
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
                  _libPipeServer.BroadCast(new UnitCmd("refreshView", "all", responseStatus: WebHelper.RespSuccess));
               }
               break;
         }
      }

      public void Start()
      {
         CancellationTokenSource cts = new CancellationTokenSource();
         _libPipeServer.RunForever(cts.Token);
      }

      public UnitCmd ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitCmd message = null)
      {
         try
         {
            
            if (msgType != LibCommProtoMsgType.Msg || message == null)
               return null;
            //if(message.OpCode != WebFrameWork.CMD_GET_LINES)
            //   _logger?.Debug($"processing, {message.OpCode}");
            switch (message.OpCode)
            {  
               case WebFrameWork.CMD_SET_REGISTER:
                  {  
                     message.Status = WebHelper.RespSuccess;
                     var registration = Utils.FromJson<RegisterClient>(message.Value.ToString());
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_REGISTER)}, already registered: {_viewData.ContainsKey(registration.Name)}, client: {registration.Name}");
                     if (!_viewData.ContainsKey(registration.Name))
                     {  
                        _viewData[registration.Name] = new WebFrame(registration.Name, _engine.GetMasterFrame());
                     }

                     //return GetLogLines(registration.Name, 0, 10);
                     return  new UnitCmd(WebFrameWork.CMD_SET_INFO, message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess) ;
                  }
               case WebFrameWork.CMD_SET_FILES:
                  {
                     var infoOfDiskFiles = message.GetData<UnitCmdAddDiskFiles>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_FILES)}, files {infoOfDiskFiles.FilePaths.Length}");                     
                     _engine.AddFiles(paths: infoOfDiskFiles.FilePaths, processAsync: true, addFileAsync: true);
                     break;
                  }
               case WebFrameWork.CMD_BST_NEW_VIEW:
                  {  
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_BST_NEW_VIEW)}, setting new view on {message.UID}, getting master frame");
                     _viewData[message.UID].SetFrame(_engine.GetMasterFrame());
                     return new UnitCmd(WebFrameWork.CMD_SET_INFO, message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_SET_INFO:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_INFO)}, setting info.");
                     return new UnitCmd(WebFrameWork.CMD_SET_INFO, message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_GET_LINES:
                  {
                     var lineInfo = message.GetData<UnitCmdGetLines>();
                     return GetLogLines(message.UID, lineInfo.StartLine, lineInfo.Length);                     
                  }
               case WebFrameWork.CMD_SET_FILTER:
                  {
                     var filter = message.GetData<TermFilter>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_FILTER)}, {filter}");
                     _viewData[message.UID].SetFrameAndFilter(_engine.Filter(filter), filter);
                     return new UnitCmd(WebFrameWork.CMD_SET_INFO, message.UID, _viewData[message.UID], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_SET_CLEAR:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_CLEAR)}, clear all");
                     _engine.Clear();
                     BroadCast(new UnitCmd(opCode: WebFrameWork.CMD_BST_NEW_VIEW, name: WebFrameWork.CLIENT_BST_ALL));
                     return null;
                  }
               case WebFrameWork.CMD_GET_FILTER_THEME_INFO:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_GET_FILTER_THEME_INFO)}, ");
                     _libPipeServer.Send(clientInfo, new UnitCmd(WebFrameWork.CMD_SET_FILTER_THEME_INFO, message.UID,
                        new UnitCmdGetThemeFilterVersionInfo()
                        {
                           VersionInfo = _commonFunctionality.GetVersionString(),
                           ListFilterNames = _commonFunctionality.GetFilters(),
                           ListThemeNames = _commonFunctionality.GetThemes(),
                           CurrentConfigUI = _commonFunctionality.GetTheme(CommonFunctionality.UserDefaultThemeName)
                        }, responseStatus: WebHelper.RespSuccess));
                     BroadCast(new UnitCmd(opCode: WebFrameWork.CMD_BST_NEW_THEME, name: WebFrameWork.CLIENT_BST_ALL));
                     return null;
                  }
               case WebFrameWork.CMD_SAVE_CONFIG:
                  _logger?.Debug($"{nameof(WebFrameWork.CMD_SAVE_CONFIG)}, ");
                  var config = message.GetData<ConfigUI>();
                  _commonFunctionality.SaveTheme(config);
                  _libPipeServer.Send(clientInfo, new UnitCmd(WebFrameWork.CMD_SET_FILTER_THEME_INFO, message.UID,
                        new UnitCmdGetThemeFilterVersionInfo()
                        {
                           VersionInfo = _commonFunctionality.GetVersionString(),
                           ListFilterNames = _commonFunctionality.GetFilters(),
                           ListThemeNames = _commonFunctionality.GetThemes(),
                           CurrentConfigUI = _commonFunctionality.GetTheme(CommonFunctionality.UserDefaultThemeName)
                        }, responseStatus: WebHelper.RespSuccess));                  BroadCast(new UnitCmd(opCode: WebFrameWork.CMD_BST_NEW_THEME, name: WebFrameWork.CLIENT_BST_ALL));
                  return null;
               case WebFrameWork.CMD_CLIENT_BST:
                  _logger?.Debug($"{nameof(WebFrameWork.CMD_CLIENT_BST)}, broadcast request.");
                  var msg = message.GetData<UnitCmd>();
                  BroadCast(msg);
                  return null;

               case WebFrameWork.CMD_SAVE_FILTER:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SAVE_FILTER)}, broadcast request.");
                     var filter = message.GetData<TermFilter>();
                     _commonFunctionality.SaveFilter(filter.Name, filter);                     
                     BroadCast(new UnitCmd(opCode: WebFrameWork.CMD_BST_NEW_THEME, name: WebFrameWork.CLIENT_BST_ALL));
                     break;
                  }

               default:
                  //message.Value = null;
                  //message.Status = WebHelper.RespFailed;
                  _logger?.Error($"{message.OpCode}, Command does not exists.");
                  SetFailed(message, $"{message.OpCode}, Command does not exists.");
                  return message;

            }

         }
         catch (Exception ex)
         {
            _logger.Error($"{msgType}, {clientInfo.ID}");
            _logger.Error(ex.ToString());
            message.Value = null;
            message.Status = WebHelper.RespFailed;
            return message;
         }

         //message.Value = null;
         //message.Status = WebHelper.RespSuccess;
         //return message;
         return null;
      }

      private UnitCmd GetLogLines(string uid, int startLine, int length) {
         var lines = new WebLogLines(uid, length);
         lines.StartLogLine = startLine;
         for (int ndx = startLine; ndx < (startLine+ length); ndx++)
         {
            if (ndx >= _viewData[uid].TotalLogLines)
            {
               break;
            }
            lines.LogLines.Add(_engine.GetLogLine(_viewData[uid].Frame.TranslateLine(ndx)));
         }
         return new UnitCmd(WebFrameWork.CMD_SET_LINES, uid, lines, responseStatus: WebHelper.RespSuccess);
      }

      private void SetFailed(UnitCmd message, string reason)
      {
         message.Value = new Dictionary<string, object>() { { "reason", reason } };
         message.Status = WebHelper.RespFailed;
      }

      public void ProcessTotalClients(int totalClients)
      {
         if (totalClients == 0)
         {
            _logger?.Info($"Detected no clients connected, signal to close. exitOnLastClient {_exitOnLastClient}");
            //TODO : enable the ability to close a server.
            //if(_exitOnLastClient)
            //   _libWebServer.Stop();
         }
      }

      public static void Test()
      {
         //var client = new WebSocketClientTest(
         //   bufferSize:4096, 
         //   logger: new WebLogProxy(new NamedLogger("ws")));
         //client.ProcessWebSocket().Wait();
      }

      public void ProcessUnitCmd(HttpListenerContext context)
      {
         throw new NotImplementedException();
      }

      public void TotalClients(long noOfClients)
      {
         _logger?.Error("Not handlering TotalCliets");
      }

      public void BroadCast(UnitCmd cmd)
      {
         if (cmd == null) {
            _logger?.Error("Trying to send null");
            return;
         }
         if (cmd.OpCode == WebFrameWork.CMD_BST_NEW_VIEW)
         {
            foreach (var view in _viewData)
            {
               view.Value.SetFrame(_engine.GetMasterFrame());
            }
         }

         _logger?.Debug($"OpCode : {cmd.OpCode}");
         _libPipeServer.BroadCast(cmd);
      }
   }


}