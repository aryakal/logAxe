
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Drawing;

using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using logAxeEngine.Engines;


namespace logAxe
{
    class ViewCommon
    {
        private static MessageExchangeHelper _msgHelper;
        public static string VersionNo { get; set; }
        //public static Bitmap Stack { get; set; }

        public static void Init()
        {
            LoadConfiguration();

            //Assembly assembly = Assembly.GetExecutingAssembly();
            var version = Assembly.GetEntryAssembly().GetName().Version;
            VersionNo = $"{version.Major}.{version.Minor}.{version.Build}";

            //Stack = global::logAxe.Properties.Resources.ConePreview_16x;

            var engine = new LogAxeEngineManager();
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
        private static string ConfigFileName { get; set; } = "config.json";
        private static frmConfigAbout ConfigFrm { get; set; }

        public static void ShowPropertyScreen()
        {
            ConfigFrm.ShowDialog();
        }

        public static void SaveConfiguration()
        {
            File.WriteAllText(ConfigFileName, JsonConvert.SerializeObject(UserConfig, Formatting.Indented));
        }

        public static void LoadConfiguration()
        {
            if (File.Exists(ConfigFileName))
            {
                UserConfig = JsonConvert.DeserializeObject<UserConfig>(File.ReadAllText(ConfigFileName));
            }
        }
        #endregion

        #region New Window
        private static int NewWindowCount = 1;
        private static Dictionary<int, frmMainWindow> _windows = new Dictionary<int, frmMainWindow>();
        public static void StartMain()
        {
            NewWindowCount++;
            var frm = new frmMainWindow();
            frm.UniqueFrmId = NewWindowCount;
            _windows.Add(NewWindowCount, frm);
            frm.Show();
        }
        public static void CloseForm(int id)
        {
            _windows.Remove(id);
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
