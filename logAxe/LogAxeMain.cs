//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Windows.Forms;

namespace logAxe
{
   static class LogAxeMain
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main()
      {

         //#if DEBUG
         //            Utils.PublishLogs = true;
         //            Utils.PublishDebugLogs = true;
         //#endif

         if (Environment.OSVersion.Version.Major >= 6)
            SetProcessDPIAware();

         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         ViewCommon.Init();
         Application.Run(new frmMainWindow());
         ViewCommon.DeInit();
      }

      [System.Runtime.InteropServices.DllImport("user32.dll")]
      private static extern bool SetProcessDPIAware();
   }
}
