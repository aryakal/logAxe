using System;
using System.Diagnostics;

namespace logAxeW
{
   static class logAxe
   {
      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main(string [] args)
      {
         Process proc = new Process();
         proc.StartInfo.UseShellExecute = true;
         proc.StartInfo.CreateNoWindow = true;
         proc.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
         proc.StartInfo.FileName = @"logAxeEngine.exe";
         proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
         
         proc.StartInfo.Arguments = string.Join(" ", args)+" --http --debug-color";   
         proc.StartInfo.RedirectStandardError = false;
         proc.StartInfo.RedirectStandardOutput = false;
       
         proc.Start();
         //proc.WaitForExit();
      }
   }
}
