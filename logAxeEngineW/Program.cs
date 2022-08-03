//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace logAxeEngineW
{
   static class Program
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string [] args)
      {
         //Start the applicaiton and die, or hold on !
         StartLogAxeEngine(args);

      }

      public static void StartLogAxeEngine(string[] args) {
         var exePath = @"logAxeEngine.exe";

         if (!File.Exists(exePath))
         {
            return;
         }

         Process proc = new Process();
         proc.StartInfo.UseShellExecute = true;
         proc.StartInfo.CreateNoWindow = true;
         proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
         proc.StartInfo.FileName = exePath;
         proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

         proc.StartInfo.Arguments = string.Join(" ", args) + " --http --debug-color";
         proc.StartInfo.RedirectStandardError = false;
         proc.StartInfo.RedirectStandardOutput = false;

         proc.Start();
         proc.WaitForExit();
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
}
