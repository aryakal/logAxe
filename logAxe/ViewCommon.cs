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
   partial class ViewCommon
   {
      //TODO : set version information.
      public static string VersionNo { get; set; } = "Version yet to come";
      public static void DeInit()
      {
         try
         {
            _logAxeEngineProcess?.Kill();
         }
         catch {
         }
      }

      public static TaskCompletionSource<bool> WaitingForInitComplete = new TaskCompletionSource<bool>();
      public static void OnNewCmd(UnitCmd message) {
         switch (message.OpCode) {
            case WebFrameWork.CMD_SET_FILTER_THEME_INFO:
               var info = message.GetData<UnitCmdGetThemeFilterVersionInfo>();
               VersionNo = info.VersionInfo;
               _currentConfig = info.CurrentConfigUI;
               _savedFilterNames.Clear();
               _savedFilterNames.AddRange(info.ListFilterNames);
               WaitingForInitComplete.SetResult(true);
               break;
         }

      }

      //public static ILogEngine Engine { get; set; }
      //public static IMessageBroker MessageBroker { get; set; }

      //public static void BroadCastMessage(ILogAxeMessage msg)
      //{
      //   _msgHelper.PostMessage(msg);
      //}

      #region User_config        
      
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
         TotalWindows = _windows.Count+1;
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

      #region SavedFilters
      private static List<string> _savedFilterNames = new List<string>();
      public static void AddFilter(string name, TermFilter filter)
      {
         if (name == "")
            return;
         filter.Name = name;
         Channel.SendMsg(new UnitCmd(opCode: WebFrameWork.CMD_SAVE_FILTER, name: ViewCommonName, value: filter));
      }

      //public static void RemoveFilter(string name)
      //{
      //   var filePath = Path.Combine(PathSavedFilterRoot, $"{name}{_filterExtension}");
      //   if (File.Exists(filePath))
      //   { 
      //      File.Delete(filePath);
      //      LoadAndCacheSavedFilterName();
      //   }
      //   //_msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.FilterChanged });
      //}

      public static string[] GetAllFilterNames()
      {
         return _savedFilterNames.ToArray();
      }

      //public static TermFilter GetFilter(string name, string uid)
      //{
         
      //}
      #endregion


   }


   partial class ViewCommon {
      private readonly static string ViewCommonName = "ViewCommonLogAxe";
      public static string DefaultDateTimeFmt { get; } = "yyyy-MM-dd hh:mm:ss.fff";
      public  static string DefaultDateTimeFileFmt { get; } = "yyyyMMddhhmmssfff";
      public static Communication Channel { get; private set; }
      public static string GenerateSeed = CommonFunctionality.ServerPipeRootPath;

      #region User Config
      private static ConfigUI _currentConfig;
      public static ConfigUI GetConfig()
      {
         return _currentConfig;
      }

      public static void SetConfig(ConfigUI config) {         
         _currentConfig = config;
         //Channel.SendMsg(
         //   new UnitCmd(opCode: WebFrameWork.CMD_CLIENT_BROADCAST, name: ViewCommonName, value: 
         //   new UnitCmd(opCode: WebFrameWork.CMD_BROADCAST_NEW_THEME, name: WebFrameWork.CLIENT_BROADCAST)
         //   ));
         Channel.SendMsg(new UnitCmd(opCode: WebFrameWork.CMD_SAVE_CONFIG, name: ViewCommonName, value: _currentConfig));
      }

      #endregion

#region ConfigAbout screen
      private static frmConfigAbout ConfigFrm { get; set; }
      public static void ShowPropertyScreen()
      {
         ConfigFrm.ShowDialog();
      }
#endregion

#region StartLogAxeEngine
      private static Process _logAxeEngineProcess;
      private static void LaunchLogAxe()
      {
         GenerateSeed = $"pipe-{DateTime.Now.ToString(DefaultDateTimeFileFmt)}";
         var startInfo = new ProcessStartInfo();
#if DEBUG
         startInfo.FileName = @"..\..\..\logAxeEngine\bin\Debug\logAxeEngine.exe";
         startInfo.Arguments = $"--debug-all --server-pipe {GenerateSeed}";
         //startInfo.RedirectStandardOutput = true;
         //startInfo.RedirectStandardError = true;
         //startInfo.UseShellExecute = false;
         //startInfo.CreateNoWindow = true;
#else
         startInfo.FileName = "logAxeEngine.exe ";
         startInfo.Arguments = $"--debug-all --server-pipe {GenerateSeed}";
         startInfo.RedirectStandardOutput = true;
         startInfo.RedirectStandardError = true;
         startInfo.UseShellExecute = false;
         startInfo.CreateNoWindow = true;
#endif
         _logAxeEngineProcess = new Process();
         _logAxeEngineProcess.StartInfo = startInfo;
         _logAxeEngineProcess.EnableRaisingEvents = true;
         _logAxeEngineProcess.Start();
         
      }
#endregion
      //private static ILibALogger _logger;
      public static void Init2()
      {
         
         LaunchLogAxe();
         ConfigFrm = new frmConfigAbout();         
         Channel = new Communication(GenerateSeed);
         Channel.Connect();
         _currentConfig = new ConfigUI();
         Channel.RegisterClient(ViewCommonName, ViewCommon.OnNewCmd);
         Channel.SendMsg(new UnitCmd(opCode: WebFrameWork.CMD_GET_FILTER_THEME_INFO, name: ViewCommonName));
      }
      public static void DeInit2()
      {
         Channel.UnRegisterClient(ViewCommonName);
         Channel.Diconnect();
      }
   }

   public sealed class HelperAttachFileDrop
   {
      public HelperAttachFileDrop(Control cntrl)
      {
         cntrl.AllowDrop = true;
         cntrl.DragDrop += OnDragDrop;
         cntrl.DragEnter += OnDragEnger;
      }

      private void OnDragEnger(object sender, DragEventArgs e)
      {
         //https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/walkthrough-performing-a-drag-and-drop-operation-in-windows-forms?view=netframeworkdesktop-4.8
         if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
         else
            e.Effect = DragDropEffects.None;
      }

      private void OnDragDrop(object sender, DragEventArgs e)
      {
         string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
         ViewCommon.Channel.SendMsg(new UnitCmd(opCode: WebFrameWork.CMD_SET_FILES, name: "all", value: new UnitCmdAddDiskFiles() { FilePaths = fileList}));
         // TODO : this is easy peasy
         //ViewCommon.Engine.AddFiles(paths: fileList, processAsync: true, addFileAsync: true);
         //ViewCommon.Engine.AddFiles(fileList, false);
      }
   }

   public class DrawSurface
   {
      public Graphics gc;
      public Bitmap bmp;
      public bool SetSize(Size size)
      {
         if (size.Width == 0 || size.Height == 0) return false;
         if (bmp != null && bmp.Size == size) return false;

         bmp = new Bitmap(size.Width, size.Height);
         gc = Graphics.FromImage(bmp);
         return true;
      }
   }

   public enum CntrlTextViewerMsg { 
      SetTitle,
      AwakeWindows

   }
}
