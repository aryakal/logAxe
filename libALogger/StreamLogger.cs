//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace libALogger
{
   public class StreamLogger : ILibAHandler
   {
      public ConsoleColor DefaultColor = Console.ForegroundColor;
      private void Log(ConsoleColor color, string msg)
      {
         Console.ForegroundColor = color;
         Console.WriteLine(msg);
         Console.ForegroundColor = DefaultColor;
      }
      public void Debug(string message)
      {
         Log(DefaultColor, message);
      }

      public void Error(string message)
      {
         Log(ConsoleColor.Red, message);
      }

      public void Info(string message)
      {
         Log(ConsoleColor.Green, message);
      }
   }
}
