//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;

namespace libALogger
{
   public class NamedLogger : ILibALogger
   {
      private static DateTime StartTime = DateTime.Now;
      public static bool PublishLogs { get; set; } = true;
      public static bool PublishDebugLogs { get; set; } = true;
      public static bool PublishConsoleLogs { get; set; } = true;
      public static ConsoleColor DefaultColor = Console.ForegroundColor;

      public string _name;
      public NamedLogger(string name, LoggingLevel level = LoggingLevel.Debug)
      {
         _name = name.Length >= 6 ? name.Substring(0, 6) : name.PadLeft(6, ' ');
      }

      public void Debug(string msg)
      {
         if (PublishDebugLogs)
            Log("D", _name, msg, DefaultColor);
      }
      public void LogDebugProgress(string msg)
      {
         if (PublishDebugLogs)
            Log("D", _name, msg, DefaultColor, true);
      }
      public void Info(string msg)
      {
         Log("I", _name, msg, ConsoleColor.White);
      }
      public void Error(string msg)
      {
         Log("E", _name, msg, ConsoleColor.Red);
      }

      private static bool usedProgressBar = false;

      private static void Log(string msgType, string grp, string msg, ConsoleColor color, bool isProgressing = false)
      {

         if (PublishLogs)
         {
            var time = $"{(DateTime.Now - StartTime).TotalSeconds:0.00}".PadLeft(6, ' ');
            //var outMsg = $"{msgType}, {time}, {Utils.GetAppMemSize()}, {grp}, {msg}";
            var outMsg = $"{msgType}, {time}, {grp}, {msg}";
            if (PublishConsoleLogs)
            {
               Console.ForegroundColor = color;
               if (isProgressing)
               {
                  Console.Write("\r" + outMsg);
                  usedProgressBar = true;
               }
               else
               {
                  if (usedProgressBar)
                  {
                     Console.Write("\n");
                     usedProgressBar = false;
                  }
                  Console.WriteLine(outMsg);
               }
               Console.ForegroundColor = DefaultColor;
            }
            else
            {
               Console.WriteLine(outMsg);
            }
         }

      }
   }
}
