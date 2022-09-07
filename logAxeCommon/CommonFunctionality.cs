//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using libALogger;
using System.Collections.Generic;
using System.Linq;

namespace logAxeCommon
{
   public class CommonFunctionality
   {
      public static string ServerPipeRootPath = "logAxe-pipe-server";
      public const string UserDefaultThemeName = "current";

      private ILibALogger _logger;
      public string RootAppDataPath { get; private set; }
      public string PathSavedThemes { get; private set; }
      public string PathSavedFilterRoot { get; private set; }
      public ConfigUI UserConfig { get; private set; }
      
      private SaveConfiguration<ConfigUI> _themes;
      private SaveConfiguration<TermFilter> _filters;
      public CommonFunctionality(string configPath=".", ILibALogger logger = null)
      {
         // When starting the program we need to know where to write the config file and store the filters.
         // If there is a path defined by user then we will use that folder otherwise as of now on windows
         // we should use the application data folder to store the log axe data
         var absPath = 
            configPath != "." ? 
            Path.GetFullPath(configPath) :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "logAxe-data");

         Init(absPath, logger);
      }
      private void Init(string configPath, ILibALogger logger = null) {
         _logger = logger;
         RootAppDataPath = configPath;
         PathSavedFilterRoot = Path.Combine(RootAppDataPath, "filters");
         PathSavedThemes = Path.Combine(RootAppDataPath, "themes");         
         _logger?.Info($"configRoot, {RootAppDataPath}");
         _logger?.Info($"    filter, {PathSavedFilterRoot}");
         _logger?.Info($"    themes, {PathSavedThemes}");
         _themes = new SaveConfiguration<ConfigUI>("theme", PathSavedThemes, Logging.GetLogger("theme"));
         _filters = new SaveConfiguration<TermFilter>("filter", PathSavedFilterRoot, Logging.GetLogger("filter"));

         _themes.Load();
         _filters.Load();

         if (! _themes.GetList().Contains(UserDefaultThemeName)) {
            _themes.SaveConfig(UserDefaultThemeName, new ConfigUI());
         }
         UserConfig = _themes.ReadConfig(UserDefaultThemeName);

         
      }
      public string GetVersionString()
      {
         var version = Assembly.GetEntryAssembly().GetName().Version;
         return $"{version.Major}.{version.Minor}.{version.Build}";
      }

      public string[] GetThemes()
      {
         return _themes.GetList();
      }

      public ConfigUI GetTheme(string name)
      {
         return _themes.ReadConfig(name);
      }

      public void ApplyTheme(ConfigUI config) {
         UserConfig = config;
      }

      public void SaveTheme(ConfigUI config)
      {
         UserConfig = config;
         _themes.SaveConfig(UserDefaultThemeName, config);
      }

      public string[] GetFilters() {
         return _filters.GetList();
      }

      public TermFilter GetFilter(string name)
      {         
         return _filters.ReadConfig(name);
      }

      public void SaveFilter(string name, TermFilter value) {
         
         _filters.SaveConfig(name, value);
         _filters.Load();
      }

   }

   public class SaveConfiguration<T> {
      private string _rootPath;
      private string _prefix;
      private string _ext = ".json";
      private ILibALogger _logger;
      private Dictionary<string, object> _filterNames = new Dictionary<string, object>();
      private Formatting _formatting;

      public SaveConfiguration(string prefix, string rootPath, ILibALogger logger=null, Formatting formatting = Formatting.Indented)
      {
         _logger = logger;
         _rootPath = rootPath;
         _prefix = prefix;
         _formatting = formatting;
      }

      public string[] Load()
      {

         _logger?.Info($"loading {_prefix},  {_rootPath}");
         if (!Directory.Exists(_rootPath))
         {
            _logger?.Info($"creating directory {_rootPath}");
            Directory.CreateDirectory(_rootPath);
         }
         _filterNames.Clear();
         
         //TODO : do better work with regex.
         foreach (var filePath in Directory.GetFiles(_rootPath))
         {
            var fileName = Path.GetFileName(filePath);
            if (fileName.StartsWith(_prefix) && fileName.EndsWith(_ext))
            {
               var name = fileName.Substring(_prefix.Length + 1).Replace(_ext, "");
               _logger?.Debug($"loading {_prefix}, {name}");
               _filterNames.Add(name, null);
            }
         }
         return _filterNames.Keys.ToArray();
      }



      public string[] GetList()
      {
         _logger?.Info($"GetList {_filterNames.Count}");
         return _filterNames.Keys.ToArray();
      }

      public void SaveConfig(string name, T value) {
         var filePath = Path.Combine(_rootPath, $"{_prefix}_{name}{_ext}");
         _filterNames[name] = value;
         _logger?.Info($"Saving {filePath}");
         File.WriteAllText(filePath, JsonConvert.SerializeObject(value, _formatting));
      }

      public T ReadConfig(string name)
      {
         var filePath = Path.Combine(_rootPath, $"{_prefix}_{name}{_ext}");
         if (_filterNames.ContainsKey(name))
         {
            if (_filterNames[name] == null)
            {
               _filterNames[name] = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            }
            return (T)_filterNames[name];
         }
         else {
            throw new Exception($"Key {name} not found");
         }
      }

      public void DeleteConfig(string name)
      {
         var filePath = Path.Combine(_rootPath, $"{_prefix}{name}{_ext}");
         if (File.Exists(filePath))
         {
            File.Delete(filePath);
         }
      }

      //public void RenameConfig(string name, string toName)
      //{
      //   var filePath = Path.Combine(_rootPath, $"{_prefix}{name}{_ext}");
      //   var filePathTo = Path.Combine(_rootPath, $"{_prefix}{toName}{_ext}");
      //   if (File.Exists(filePath))
      //   {
      //      File.Replace(filePath, filePathTo);
      //   }
      //}
   }
}
