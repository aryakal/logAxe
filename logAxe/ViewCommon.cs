//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using logAxeCommon;
using libALogger;
using libACommunication;
using System.Diagnostics;

namespace logAxe
{
   class ViewCommon
   {
      public static string VersionNo { get; set; } = "";
      public static string DefaultDateTimeFmt { get; } = "yyyy-MM-dd hh:mm:ss.fff";
      public static string DefaultDateTimeFileFmt { get; } = "yyyyMMddhhmmssfff";
      public static Communication Channel { get; private set; }

      public static string GenerateSeed = CommonFunctionality.ServerPipeRootPath;

      #region Init and Deinit
      public static void Init(bool showConsole)
      {
         VersionNo = CommonFunctionality.GetLogAxeVersion();
         LaunchLogAxe(showConsole);
         _uiConfigFrm = new frmConfigAbout();
         _uiFileManagerFrm = new frmFileManager();
         Channel = new Communication(GenerateSeed);
         Channel.Connect();
         
         //Channel.RegisterClient(ViewCommonName, ProcessCommandsFromChannel);
         //Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_FILTER_THEME_INFO, name: ViewCommonName));
      }
      public static void DeInit()
      {

         try
         {
            _logAxeEngineProcess?.Kill();
            Channel.Diconnect();
         }
         catch (InvalidOperationException)
         { 
         }
      }
      #endregion

      #region User Config
      
      public static ConfigUI ConfigOfSystem { get; set; } = new ConfigUI();

      #endregion

      #region ConfigAbout screen
      private static frmConfigAbout _uiConfigFrm { get; set; }

      public static void ShowPropertyScreen()
      {
         _uiConfigFrm.ShowDialog();
      }
      
      #endregion

      #region ConfigAbout screen
      private static frmFileManager _uiFileManagerFrm { get; set; }
      public static void ShowFileManager()
      {
         _uiFileManagerFrm.ShowDialog();
      }
      #endregion

      #region StartLogAxeEngine
      private static Process _logAxeEngineProcess;
      private static void LaunchLogAxe(bool showConsole)
      {
         GenerateSeed = $"pipe-{DateTime.Now.ToString(DefaultDateTimeFileFmt)}";
         var startInfo = new ProcessStartInfo();
#if DEBUG
         startInfo.FileName = @"..\..\..\logAxeEngine\bin\Debug\logAxeEngine.exe";
#else
         startInfo.FileName = "logAxeEngine.exe ";    
         startInfo.RedirectStandardOutput = true;
         startInfo.RedirectStandardError = true;
         startInfo.UseShellExecute = false;
         startInfo.CreateNoWindow = true;
#endif
         startInfo.Arguments = showConsole ? $"--debug-all --server-pipe {GenerateSeed} --close-noclient" : $"--server-pipe {GenerateSeed} --close-noclient";
         startInfo.RedirectStandardOutput = !showConsole;
         startInfo.RedirectStandardError = !showConsole;
         startInfo.UseShellExecute = showConsole;
         startInfo.CreateNoWindow = !showConsole;

         _logAxeEngineProcess = new Process();
         _logAxeEngineProcess.StartInfo = startInfo;
         _logAxeEngineProcess.EnableRaisingEvents = true;
         _logAxeEngineProcess.Start();

      }
      #endregion

      #region New Window
      /*
       * If the window is open then we would like to knwo which window it is this. 
       */
      private static Dictionary<string, frmMainWindow> _windows = new Dictionary<string, frmMainWindow>();
      public static int TotalWindows = 1;
      private static int NextId = 0;
      public static void StartMain()
      {
         NextId++;
         var frm = new frmMainWindow();         
         frm.FrmID = $"View {NextId}";         
         _windows.Add(frm.FrmID, frm);
         frm.Register(isMainView: false);

         TotalWindows = _windows.Count + 1;
         frm.Show();
         //_msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewMainFrmAddRemoved });
      }
      public static void CloseForm(string id)
      {
         _windows.Remove(id);
         TotalWindows = _windows.Count + 1;
         //_msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewMainFrmAddRemoved });
      }
      #endregion

      #region New notepad window
      private static int NewNotepadCount = 0;
      private static Dictionary<string, frmNotepad> _notepads = new Dictionary<string, frmNotepad>();
      public static string OpenNewNotepad()
      {
         NewNotepadCount++;
         var notepadName = $"Notepad [{NewNotepadCount}]";
         _notepads.Add(notepadName, null);
         var frm = new frmNotepad();
         frm.Register();
         frm.NotepadName = notepadName;
         frm.SetTitle(frm.NotepadName);
         frm.Show();
         _notepads[notepadName] = frm;
         //_msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NotepadAddedOrRemoved });
         return notepadName;


      }
      public static void CloseNotepad(string name)
      {
         _notepads.Remove(name);
         //_msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NotepadAddedOrRemoved });
      }
      public static List<string> GetAllNotepadNames()
      {
         return _notepads.Keys.ToList();
      }
      #endregion
      

      #region interproc command
      public static void AddFiles(string viewName, string[] fileList)
      {
         ViewCommon.Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_PUT_FILES, name: viewName, value: new UnitCmdAddDiskFiles() { FilePaths = fileList }));
      }

      public static void ExportFiles(string viewName, LogFileInfo[] fileInfo, string filename)
      {
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_EXPORT_FILES, name: viewName, value: new UnitCmdExportFile()
         {
            Files = fileInfo,
            ExportFileName = filename
         }));
      }

      public static void GetFileAppMemInfo(string viewName)
      {
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.MSG_BST_PROGRESS, name: viewName));
      }

      public static void GetCurrentTheme(string viewName) {
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_CONFIG_CURRENT, name: viewName));
      }

      public static void PostCurrentTheme(ConfigUI config)
      {
         ViewCommon.ConfigOfSystem = config;
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_POST_CONFIG, name: WebFrameWork.CLIENT_BST_ALL, value: config));
      }

      public static void GetFilterList(string viewName)
      {
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_GET_FILTER_LIST, name: viewName));
      }

      public static void AddFilter(string viewName, string filterName, TermFilter filter)
      {
         filter.Name = filterName;
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.CMD_POST_FILTER_SAVE, name: viewName, value: filter));
      }

      public static void MsgBroadcastGlobalLine(string viewName, int globalLine)
      {  
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.MSG_GLOBAL_LINE, name: viewName, value: globalLine));
      }

      public static void ExportLinesToClipBoard(string viewName, int [] lines)
      {
         Channel.SendMsg(new UnitMsg(opCode: WebFrameWork.MSG_COPY_TO_CLIPBOARD, name: viewName, value: lines));
      }

      #endregion
   }

}
