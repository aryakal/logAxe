//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using logAxeEngine.Engines;
using System.IO;

using logAxeCommon;
using logAxe.http;
using System.Net.WebSockets;
using System.Threading;
using libACommunication;
using libALogger;
using System.Threading.Tasks;

namespace logAxeEngine
{
   class LogAxeEngineMain
   {
      static void Main(string[] args)
      {
         ILibALogger logger = Logging.GetLogger("main"); ;
         logger.Info("Starting program");
         try
         {
            new LogAxeEngineMain().Init(args, logger);
         }
         catch (Exception ex) 
         {
            logger.Error(ex.ToString());
         }
         logger.Info("Stoping program");
      }
      private void Init(string [] args, ILibALogger logger)
      {
         var cmd_workspace = "--wkp";
         var cmd_workspace_local = "--wkp-win";
         var cmd_debug_all = "--debug-all";
         var cmd_debug = "--debug";
         //var cmd_debug_http = "--debug-http";
         var cmd_port = "--port";
         var cmd_start_browser = "--start-browser";
         var cmd_server_pipe = "--server-pipe";
         //var cmd_server_http = "--server-http";


         var cmdParser = new CmdParser();
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_workspace, ValueType = typeof(string), DefaultValue = ".", CmdHelper = "configuration file to be used by log axe" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_workspace_local, ValueType = typeof(string), DefaultValue = ".", CmdHelper = "configuraiton to use user local directory" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_debug_all, ValueType = typeof(bool), CmdHelper = "enables all debug" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_debug, ValueType = typeof(bool), CmdHelper = "enable prints debug message" });         
         //cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_debug_http, ValueType = typeof(bool), CmdHelper = "bool, prints the log from libWebServer" });
         //cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_server_http, ValueType = typeof(bool), CmdHelper = "bool, starts http sever" });
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_port, ValueType = typeof(int), DefaultValue = 8080, CmdHelper = "default 8080 using this as the default port number to start with." });
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_start_browser, ValueType = typeof(bool), CmdHelper = "starts http sever" });         
         cmdParser.AddCommand(new CmdInfo() { Cmd = cmd_server_pipe, ValueType = typeof(string), DefaultValue = CommonFunctionality.ServerPipeRootPath, CmdHelper = "start pipe server" , EnableUseDefault=true});
         cmdParser.Parse(args);

         logger?.Debug($"Working directory {Directory.GetCurrentDirectory()}");
         if (cmdParser.Proceed)
         {
            if (cmdParser.IsEnabled(cmd_debug_all))
            {
               cmdParser.SetEnabledValue(cmd_debug, true);               
               //cmdParser.SetEnabledValue(cmd_debug_http, true);
            }

            NamedLogger.PublishDebugLogs = cmdParser.IsEnabled(cmd_debug);

            var commonFunctionality = cmdParser.IsEnabled(cmd_workspace_local)? 
               new CommonFunctionality(Logging.GetLogger("cfg")):
               new CommonFunctionality(cmdParser.GetString(cmd_workspace), Logging.GetLogger("cfg"));
            //TODO find alternative
            //commonFunctionality.LoadUserConfig();

            var messageEngine = new LogMessageEngine();
            var pluginManager = new PluginManager();
            var engine = new LogAxeEngineManager(messageEngine, pluginManager);
            engine.RegisterPlugin(".");

            //if (cmdParser.IsEnabled(cmd_server_http))
            //{
            //   var http = new logAxeHttpServerProxy(
            //      logger: Logging.GetLogger("logAxe"),
            //      engine: engine,
            //      port: cmdParser.GetInt(cmd_port),
            //      serverIp: "127.0.0.1",
            //      appRootPath: "/logAxe/",
            //      debugHttp: cmdParser.IsEnabled(cmd_debug_http));
            //   if (cmdParser.IsEnabled("--start-browser"))
            //   {
            //      http.StartWebBrowser();
            //   }
            //   http.Start();
            //}            

            if (cmdParser.IsEnabled(cmd_server_pipe))
            {
               var pipeServer = new logAxePipeServer(
                  logger: Logging.GetLogger("proxy"),
                  engine: engine,
                  cmdParser.GetString(cmd_server_pipe),
                  commonFunctionality
                  );
               pipeServer.Start();
            }
         }
      }
   }
}
