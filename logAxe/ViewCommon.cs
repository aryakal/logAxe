
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using logAxeEngine.Engines;
using System;

namespace logAxe
{
   class ViewCommon
   {
      private static string RootAppDataPath {get;set;}
      private static MessageExchangeHelper _msgHelper;
      public static string VersionNo { get; set; }
      //public static Bitmap Stack { get; set; }

      public static void Init()
      {
         RootAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "logAxe-data");
         ConfigFilePath = Path.Combine(RootAppDataPath, "logaxe-config.json");
         if (!Directory.Exists(RootAppDataPath))
         {
            Directory.CreateDirectory(RootAppDataPath);
         }
         LoadConfiguration();
         
         var version = Assembly.GetEntryAssembly().GetName().Version;
         VersionNo = $"{version.Major}.{version.Minor}.{version.Build}";

         //Stack = global::logAxe.Properties.Resources.ConePreview_16x;

         var engine = new LogAxeEngineManager(automaticSendMsg:true);
         Engine = engine;
         MessageBroker = engine.MessageBroker;
         Engine.RegisterPlugin(".");

         _msgHelper = new MessageExchangeHelper(ViewCommon.MessageBroker, null);

         //We use the ViewCommon.Engine inside the frm so we need to enable this before.
         ConfigFrm = new frmConfigAbout();

      }

      public static void DeInit()
      {

      }

      public static ILogEngine Engine { get; set; }
      public static IMessageBroker MessageBroker { get; set; }
      public static UserConfig UserConfig { get; set; } = new UserConfig();

      public static void BroadCastMessage(ILogAxeMessage msg)
      {
         _msgHelper.PostMessage(msg);
      }

      #region User_config        
      private static string ConfigFilePath { get; set; }
      private static frmConfigAbout ConfigFrm { get; set; }

      public static void ShowPropertyScreen()
      {
         ConfigFrm.ShowDialog();
      }

      public static void SaveConfiguration()
      {
         //TODO : backup last config !
         File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(UserConfig, Formatting.Indented));
      }

      public static void LoadConfiguration()
      {
         if (File.Exists(ConfigFilePath))
         {
            UserConfig = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(ConfigFilePath));
         }
      }
      #endregion

      #region New Window
      public static int NewWindowCount = 1;
      private static Dictionary<int, frmMainWindow> _windows = new Dictionary<int, frmMainWindow>();
      public static void StartMain()
      {
         NewWindowCount++;
         var frm = new frmMainWindow();
         frm.UniqueFrmId = NewWindowCount;
         _windows.Add(NewWindowCount, frm);
         frm.Show();
         _msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewMainFrmAddRemoved });
      }
      public static void CloseForm(int id)
      {
         _windows.Remove(id);
         _msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NewMainFrmAddRemoved });
      }      
      #endregion

      #region New Window
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
         _msgHelper.PostMessage(new LogAxeGenericMessage() { MessageType = LogAxeMessageEnum.NotepadAddedOrRemoved });
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
         ViewCommon.Engine.AddFiles(paths: fileList, processAsync: true, addFileAsync: true);
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
}
