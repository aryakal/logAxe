//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Diagnostics;
using System.Windows.Forms;

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
         if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();
         Console.WriteLine(args[0]);
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);         
         ViewCommon.Init2();
         ViewCommon.WaitingForInitComplete.Task.Wait();
         Application.Run(new frmMainWindow());         
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
