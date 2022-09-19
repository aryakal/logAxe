//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Windows.Forms;
using logAxeCommon;

namespace logAxe
{
   static class LogAxeMain
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string [] args)
      {
         var cmd_show_console = "--show-console";
         var cmdParser = new CmdParser();
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_show_console, ValueType = typeof(bool), CmdHelper = "show the console of logAxeEngine" });
         cmdParser.Parse(args);

         if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();
         
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);         
         ViewCommon.Init(cmdParser.IsEnabled(cmd_show_console));
         //ViewCommon.WaitingForInitComplete.Task.Wait();
         var frm = new frmMainWindow();
         frm.Register(isMainView:true);
         Application.Run(frm);         
         ViewCommon.DeInit();
      }

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern bool SetProcessDPIAware();
   }



   //static class logAxe
   //{
   //   /// <summary>
   //   /// The main entry point for the application.
   //   /// </summary>
   //   [STAThread]
   //   static void Main(string[] args)
   //   {
   //      Process proc = new Process();
   //      proc.StartInfo.UseShellExecute = true;
   //      proc.StartInfo.CreateNoWindow = true;
   //      proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
   //      proc.StartInfo.FileName = @"logAxeEngine.exe";
   //      proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

   //      proc.StartInfo.Arguments = string.Join(" ", args) + " --http --debug-color";
   //      proc.StartInfo.RedirectStandardError = false;
   //      proc.StartInfo.RedirectStandardOutput = false;

   //      proc.Start();
   //      //proc.WaitForExit();
   //   }
   //}
}
