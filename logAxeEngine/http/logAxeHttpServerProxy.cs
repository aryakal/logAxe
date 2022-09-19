//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;


using logAxeCommon;
using logAxeCommon.Interfaces;
using logAxeEngine.Interfaces;

using libACommunication;
using libALogger;


namespace logAxe.http
{
   public class logAxePipeServer : IProtoProcessorProcessClients, IMessageExchanger
   {
      private ILibALogger _logger;
      private ILogEngine _engine;      
      private CommonFunctionality _commonFunctionality;
      private IDLServer _libPipeServer;
      private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
      private bool _exitOnLastClient = false;
      private CancellationTokenSource _cts;
      class ViewData
      {
         public LogFrame Frame { get; set; } = null;
         public TermFilter Filter { get; set; } = new TermFilter();
      }
      Dictionary<string, WebFrame> _viewData = new Dictionary<string, WebFrame>();
      public logAxePipeServer(ILibALogger logger, ILogEngine engine, string pipeName, CommonFunctionality commonFunctionality, bool exitOnLastClient)
      {
         _logger = logger;
         _commonFunctionality = commonFunctionality;
         _engine = engine;
         _engine.OnParseComplete += () => {
            BroadCast(new UnitMsg(opCode: WebFrameWork.MSG_BST_PROGRESS, name: WebFrameWork.CLIENT_BST_ALL, value: _engine.GetAllLogFileInfo()));
            BroadCast(new UnitMsg(opCode: WebFrameWork.CMD_PUT_NEW_VIEW, name: WebFrameWork.CLIENT_BST_ALL));
         };
         _exitOnLastClient = exitOnLastClient;         
         _libPipeServer = new PipeServer(
            logger: Logging.GetLogger("srv"),
            processor: this,
            pipeName
            );
      }
      public void Start()
      {
         if (_exitOnLastClient)
         {
            _logger?.Info("will close when last client closes");
         }
         _cts = new CancellationTokenSource();
         _libPipeServer.RunForever(_cts.Token);
         _logger?.Debug("Closing");
      }
      public UnitMsg ProcessUnitCmd(LibCommProtoMsgType msgType, IClientInfo clientInfo, UnitMsg message = null)
      {
         try
         {

            if (msgType != LibCommProtoMsgType.Msg || message == null)
               return null;
            //if (message.OpCode != WebFrameWork.CMD_GET_LINES)
            //   _logger?.Debug($"processing, {message.UniqueId}, {message.OpCode}");
            switch (message.OpCode)
            {
               case WebFrameWork.CMD_PUT_REGISTER:
                  {
                     message.Status = WebHelper.RespSuccess;                     
                     var registration = message.GetData<RegisterClient>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_REGISTER)}, already registered: {_viewData.ContainsKey(registration.Name)}, client: {registration.Name}, emptyView: {!registration.IsViewRequired}");
                     if (!_viewData.ContainsKey(registration.Name))
                     {                        
                        _viewData[registration.Name] = new WebFrame(registration.Name, registration.IsViewRequired?_engine.GetMasterFrame(): LogFrame.GetEmptyView());
                     }
                     return new UnitMsg(WebFrameWork.CMD_PUT_INFO, message.UniqueId, _viewData[message.UniqueId], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_PUT_UNREGISTER:
                  {  
                     message.Status = WebHelper.RespSuccess;
                     var registration = message.GetData<RegisterClient>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_UNREGISTER)}, unregistering: {registration.Name}");
                     if (_viewData.ContainsKey(registration.Name))
                     {
                        _viewData.Remove(registration.Name);
                     }
                     break;
                  }
               case WebFrameWork.CMD_PUT_FILES:
                  {
                     var infoOfDiskFiles = message.GetData<UnitCmdAddDiskFiles>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_FILES)}, files {infoOfDiskFiles.FilePaths.Length}");
                     _engine.AddFiles(paths: infoOfDiskFiles.FilePaths, processAsync: true, addFileAsync: true);
                     
                     BroadCast(new UnitMsg(opCode: WebFrameWork.CMD_MSG_PROGRESS, name: WebFrameWork.CLIENT_BST_ALL, value:_engine.GetAllLogFileInfo()));
                     break;
                  }
               case WebFrameWork.CMD_PUT_NEW_VIEW:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_NEW_VIEW)}, setting new view on {message.UniqueId}, getting master frame");
                     _viewData[message.UniqueId].SetFrame(_engine.GetMasterFrame());
                     return new UnitMsg(WebFrameWork.CMD_PUT_INFO, message.UniqueId, _viewData[message.UniqueId], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_PUT_INFO:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_INFO)}, setting info.");
                     return new UnitMsg(WebFrameWork.CMD_PUT_INFO, message.UniqueId, _viewData[message.UniqueId], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_GET_LINES:
                  {
                     var lineInfo = message.GetData<UnitCmdGetLines>();
                     return GetLogLines(message.UniqueId, lineInfo.StartLine, lineInfo.Length);
                  }
               case WebFrameWork.CMD_SET_FILTER:
                  {
                     var filter = message.GetData<TermFilter>();
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_SET_FILTER)}, {filter}");
                     _viewData[message.UniqueId].SetFrameAndFilter(_engine.Filter(filter), filter);
                     return new UnitMsg(WebFrameWork.CMD_PUT_INFO, message.UniqueId, _viewData[message.UniqueId], responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_PUT_CLEAR:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_CLEAR)}, clear all");
                     _engine.Clear();                     
                     return new UnitMsg(opCode: WebFrameWork.CMD_PUT_NEW_VIEW, name: WebFrameWork.CLIENT_BST_ALL);
                  }

               case WebFrameWork.CMD_POST_CONFIG:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_POST_CONFIG)}, ");
                     var config = message.GetData<ConfigUI>();
                     _commonFunctionality.SaveTheme(config);                     
                     return new UnitMsg(opCode: WebFrameWork.CMD_GET_CONFIG_CURRENT, name: WebFrameWork.CLIENT_BST_ALL, value: _commonFunctionality.GetTheme(CommonFunctionality.UserDefaultThemeName));
                  }
               case WebFrameWork.CMD_PUT_CLIENT_BST:
                  _logger?.Debug($"{nameof(WebFrameWork.CMD_PUT_CLIENT_BST)}, broadcast request.");
                  var msg = message.GetData<UnitMsg>();
                  BroadCast(msg);
                  return null;
               case WebFrameWork.CMD_POST_FILTER_SAVE:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_POST_FILTER_SAVE)}, save filter.");
                     var filter = message.GetData<TermFilter>();
                     _commonFunctionality.SaveFilter(filter.Name, filter);
                     return new UnitMsg(opCode: WebFrameWork.CMD_GET_FILE_LIST, name: WebFrameWork.CLIENT_BST_ALL, value: _commonFunctionality.GetFilters());
                  }

               case WebFrameWork.CMD_GET_FILE_LIST:
                  {
                     return new UnitMsg(WebFrameWork.CMD_PUT_FILE_LIST, message.UniqueId, _engine.GetAllLogFileInfo(), responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_GET_EXPORT_FILES:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_GET_EXPORT_FILES)}, export file.");
                     var exportInfo = message.GetData<UnitCmdExportFile>();
                     _engine.ExportFiles(exportInfo.Files, exportInfo.ExportFileName);
                     break;
                  }
               case WebFrameWork.CMD_MSG_PROGRESS:
                  {
                     return new UnitMsg(WebFrameWork.CMD_MSG_PROGRESS, message.UniqueId, value: _engine.GetStartInfo(), responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.MSG_BST_PROGRESS:
                  {
                     return new UnitMsg(WebFrameWork.MSG_BST_PROGRESS, WebFrameWork.CLIENT_BST_ALL, value: _engine.GetStartInfo(), responseStatus: WebHelper.RespSuccess);
                  }

               case WebFrameWork.CMD_GET_FILTER_DETAILS:
                  {
                     var filter = message.GetData<TermFilter>();
                     filter = _commonFunctionality.GetFilter(filter.Name);
                     return new UnitMsg(WebFrameWork.CMD_PUT_FILTER_DETAILS, message.UniqueId, value: filter, responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_DEL_FILTER_DETAIL:
                  {
                     var filter = message.GetData<TermFilter>();
                     filter = _commonFunctionality.GetFilter(filter.Name);
                     return new UnitMsg(WebFrameWork.CMD_PUT_FILTER_DETAILS, message.UniqueId, value: filter, responseStatus: WebHelper.RespSuccess);
                  }
               case WebFrameWork.CMD_GET_CONFIG_CURRENT:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_GET_CONFIG_CURRENT)}, get config request.");
                     message.Value = _commonFunctionality.GetTheme(CommonFunctionality.UserDefaultThemeName);
                     message.Status = WebHelper.RespSuccess;
                     return message;
                  }
               case WebFrameWork.CMD_GET_FILTER_LIST:
                  {
                     _logger?.Debug($"{nameof(WebFrameWork.CMD_GET_FILTER_LIST)}, get full filter list.");
                     message.Value = _commonFunctionality.GetFilters();
                     message.Status = WebHelper.RespSuccess;
                     return message;
                  }
               case WebFrameWork.MSG_GLOBAL_LINE:
                  {
                     //TODO fix the reverse broadcast issue, we need to optimize.
                     _logger?.Debug($"{nameof(WebFrameWork.MSG_GLOBAL_LINE)}, navigate to global line.");
                     _logger?.Debug($"Client : {message.UniqueId}, {clientInfo.UniqueId}");
                     var globalLine = message.GetData<int>();
                     foreach (var view in _viewData)
                     {
                        if (view.Value.ViewName == message.UniqueId)
                        {
                           continue;
                        }
                        var navigateToLineWithPage = view.Value.Frame.GetGlobalLine(globalLine);                        
                        var msgNavigateLine = new UnitMsg(WebFrameWork.MSG_NAVIGATE_TO_VIEW_LINE, view.Key, value: navigateToLineWithPage, responseStatus: WebHelper.RespSuccess);
                        BroadCast(msgNavigateLine);
                     }
                     
                     return null;
                  }
               case WebFrameWork.MSG_COPY_TO_CLIPBOARD:
                  {
                     _logger?.Debug($"Client : MSG_COPY_TO_CLIPBOARD : {message.UniqueId}");
                     var clipboardLines = message.GetData<int[]>();
                     foreach (var view in _viewData)
                     {
                        if (view.Value.ViewName == message.UniqueId)
                        {
                           return ExportToClipboard(message.UniqueId, clipboardLines);
                        }
                     }
                     return null;
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
            _logger.Error($"{msgType}, {message.OpCode}, {clientInfo.UniqueId}");
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
      public void TotalClients(long noOfClients)
      {
         _logger?.Error($"total clients {noOfClients}");
         if (_exitOnLastClient && noOfClients == 0)
         {
            _logger?.Info("Closing all operations");
            _cts.Cancel();
         }
      }
      public void BroadCast(UnitMsg cmd)
      {
         if (cmd == null)
         {
            _logger?.Error("Trying to send null");
            return;
         }
         if (cmd.OpCode == WebFrameWork.CMD_PUT_NEW_VIEW)
         {
            foreach (var view in _viewData)
            {
               view.Value.SetFrame(_engine.GetMasterFrame());
            }
         }

         _logger?.Debug($"OpCode : {cmd.OpCode}");
         _libPipeServer.BroadCast(cmd);
      }
      private UnitMsg ExportToClipboard(string UniqueId, int [] lines) {
         var selectedPLines = new StringBuilder();
         var selectedHLines = new StringBuilder();

         foreach (var lineNo in lines)
         {
            var line = _engine.GetLogLine(_viewData[UniqueId].Frame.TranslateLine(lineNo));
            var logType = line.LogType.ToString().Substring(0, 1);
            //TODO : move to logAxeLibCommon the default time format.
            var timeStamp = line.TimeStamp.ToString(_commonFunctionality.GetTheme(CommonFunctionality.UserDefaultThemeName).Column1TimeStampFormat);
            var logText = line.Msg.Length > 120 ? line.Msg.Substring(0, 120).Replace("\n", "") + "..." : line.Msg.Replace("\n", "");
            var lineColor = "red";

            switch (line.LogType)
            {
               case LogType.Info:
                  lineColor = "green";
                  break;
               case LogType.Trace:
                  lineColor = "black";
                  break;
               case LogType.Warning:
                  lineColor = "orange";
                  break;

            }
            selectedPLines.Append($"{logType}, {timeStamp}, {logText}{Environment.NewLine}");
            selectedHLines.Append($"{logType}, {timeStamp}, <span style=\"color: {lineColor}\">{logText}</span><br>");


         }
         return new UnitMsg(WebFrameWork.MSG_COPY_TO_CLIPBOARD_UNTIL_FIXED_HTML, UniqueId, value: new string[] { selectedHLines.ToString(), selectedPLines.ToString() }, responseStatus: WebHelper.RespSuccess);
         
      }
      private UnitMsg GetLogLines(string uid, int startLine, int length)
      {
         var lines = new WebLogLines(uid, length);
         lines.StartLogLine = startLine;
         for (int ndx = startLine; ndx < (startLine + length); ndx++)
         {
            if (ndx >= _viewData[uid].TotalLogLines)
            {
               break;
            }
            lines.LogLines.Add(_engine.GetLogLine(_viewData[uid].Frame.TranslateLine(ndx)));
         }
         return new UnitMsg(WebFrameWork.CMD_PUT_LINES, uid, lines, responseStatus: WebHelper.RespSuccess);
      }
      private void SetFailed(UnitMsg message, string reason)
      {
         message.Value = new Dictionary<string, object>() { { "reason", reason } };
         message.Status = WebHelper.RespFailed;
      }      
   }


}