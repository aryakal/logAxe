using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using logAxeEngine.Common;
using logAxeCommon;

namespace logAxeEngine.Engines
{
   /// <summary>
   /// The manager to load and maintain the plugins.
   /// </summary>
   sealed class PluginManager
   {
      private Dictionary<string, ILogParser> _plugins = new Dictionary<string, ILogParser>();
      private StringBuilder _currentInformation = new StringBuilder();
      private NamedLogger _logger = new NamedLogger("plgMng");
      public ILogParser GuessParser(string fileName)
      {
         foreach (var plugin in _plugins)
         {
            if (plugin.Value.CanParseLogFile(fileName))
            {
               return plugin.Value;
            }
         }
         _logger.LogError($"no parser for, {fileName}");
         return null;
      }
      public string GetAllPluginsInfo()
      {
         return _currentInformation.ToString();
      }
      public void LoadFromFolder(string directoryPath)
      {
         foreach (var dllFileName in Directory.GetFiles(directoryPath))
         {

            if (dllFileName.ToLower().EndsWith("plugin.dll"))
            {
               LoadPlugin(Path.GetFullPath(dllFileName));
            }
         }
      }
      private void LoadPlugin(string filePath)
      {
         var dllInstance = Assembly.LoadFile(filePath);
         foreach (var type in dllInstance.GetExportedTypes())
         {
            if (null != type.GetInterface("ILogParser"))
            {
               var instance = dllInstance.CreateInstance($"{type}") as ILogParser;
               _plugins[instance.ParserName] = instance;
               _currentInformation.Append($"{Environment.NewLine}Current plugins loaded.{Environment.NewLine}");
               _currentInformation.Append($" > {instance.ParserName} in {filePath} {Environment.NewLine}");
               _logger.LogDebug($"loaded {filePath} @ {instance.ParserName}");
            }
         }
      }
   }
}
