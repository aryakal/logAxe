//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using logAxeEngine.Common;
using logAxeEngine.Engines;
using logAxeEngine.Interfaces;
using logAxe.http;
using System.IO;
using Newtonsoft.Json;

namespace logAxeEngine
{
   class LogAxeEngineMain
   {
      static void Main(string[] args)
      {
         try
         {
            new LogAxeEngineMain().Init(args);
         }
         catch (Exception) 
         { 
         }
      }

      private static void Process_Exited(object sender, EventArgs e)
      {
         
      }

      ILogEngine _engine;
      logAxeHttpServerProxy _http;
      LogAxeConfig _config;
      NamedLogger _logger = new NamedLogger("main");

      private void Init(string [] args)
      {
         

         var cmdParser = new CmdParser();         
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--debug", ValueType = typeof(bool), CmdHelper = "bool, prints debug message" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--debug-color", ValueType = typeof(bool), CmdHelper = "bool, prints debug message with color" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--debug-http", ValueType = typeof(bool), CmdHelper = "bool, prints the log from libWebServer" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--http", ValueType = typeof(bool), CmdHelper = "bool, starts http sever" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--start-browser", ValueType = typeof(bool), CmdHelper = "bool, starts http sever" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--port", ValueType = typeof(int), DefaultValue = 8080, CmdHelper = "int, default 8080 using this as the default port number to start with." });
         cmdParser.AddCommand(new CmdInfo() { Cmd = "--config-file", ValueType = typeof(string), DefaultValue = "logAxeConfig.json", CmdHelper = "configuration file to be used by log axe" });


         // args = new string[] { "--debug-color", "--http", "--start-browser" }; // start with browser
         // args = new string[] { "--debug", "--http", "--start-browser", "--config-file", "logAxeConfig.json" }; // start without browser       
         cmdParser.Parse(args);
         _logger.Info("Staring program");
         if (cmdParser.Proceed)
         {
            var debugEnabled = cmdParser.IsEnabled("--debug") || cmdParser.IsEnabled("--debug-color");
            NamedLogger.PublishLogs = debugEnabled;
            NamedLogger.PublishDebugLogs = debugEnabled;
            NamedLogger.PublishConsoleLogs = cmdParser.IsEnabled("--debug-color");

            _config = new LogAxeConfig();
            if (cmdParser.IsEnabled("--config-file"))
               LoadConfig(cmdParser.GetString("--config-file"));

            //Start the engine then anything else.
            
            _engine = new LogAxeEngineManager(new LogMessageEngine(), new PluginManager());
            _http = new logAxeHttpServerProxy(
               logger: new NamedLogger("http"), 
               engine: _engine,
               debugHttp: cmdParser.IsEnabled("--debug-http")
               );

            _engine.RegisterPlugin(".");            
            if (cmdParser.IsEnabled("--start-browser"))
            {
               _http.StartWebBrowser();

            }
            if (cmdParser.IsEnabled("--http"))
            {
               
               _http.Start();
            }
         }
      }
      private void LoadConfig(string configFilePath)
      {
         if (!File.Exists(configFilePath)) {
            _logger.Error($@"File not found creating file @ {configFilePath}");
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(_config, Formatting.Indented));
         };
         _config = JsonConvert.DeserializeObject<LogAxeConfig>(File.ReadAllText(configFilePath));
      }
   }

  
}
