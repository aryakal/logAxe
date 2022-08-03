//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;

namespace libALogger
{
   public static class Logging
   {
      public static ILibAHandler[] _handlers = new ILibAHandler[] { new StreamLogger()};
      public static LoggingLevel _level = LoggingLevel.Debug;
      public static bool _isDebugAllowed = true;
      
      public static ILibALogger GetLogger(string name)
      {
         return new NamedLogger(name);
      }

      public static ILibALogger GetLogger(string name, LoggingLevel level)
      {
         return new NamedLogger(name);
      }

      public static void BasicConfig(LoggingLevel level)
      {

      }
      public static void BasicConfig(LoggingLevel level, string format, ILibAHandler[] handlers)
      { 
      }
      public static void Debug(string componentName, string message)
      {
         if (_isDebugAllowed) { 
         Write(LoggingLevel.Debug, $"D, {componentName}, {message}");
         }
      }
      public static void Error(string componentName, string message)
      {
         Write(LoggingLevel.Error, $"E, {componentName}, {message}");
      }      
      public static void Info(string componentName, string message)
      {
         Write(LoggingLevel.Info, $"I, {componentName}, {message}");
      }
      private static void Write(LoggingLevel level, string message)
      {
         foreach (var handler in _handlers)
         {
            switch (level) {
               case LoggingLevel.Debug:
                  handler.Debug(message);
                  break;
               case LoggingLevel.Error:
                  handler.Error(message);
                  break;
               case LoggingLevel.Info:
                  handler.Info(message);
                  break;
            }
         }
      }
   }
}
