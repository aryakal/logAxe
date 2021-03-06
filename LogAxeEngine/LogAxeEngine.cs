using System;
using System.Collections.Generic;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using logAxeEngine.Engines;
using System.Threading;

namespace logAxeEngine
{
    class logAxeEngine
    {
        static void Main(string[] args)
        {
            NamedLogger.PublishLogs = true;
            NamedLogger.PublishConsoleLogs = true;
            NamedLogger.PublishDebugLogs = true;
            var logger = new NamedLogger("main");

            var printHelp = args.Length == 0;

            if (printHelp)
            {
                Console.WriteLine("--version        Print the version number.");
                Console.WriteLine("--create-files   Create files for testing with engine.");
                Console.WriteLine("--test           Full test engine, with files created with --create-files");
                Console.WriteLine("--execute        json files, { commands = [ {} ] } ");
            }

            //ILogEngine engine = new LogAxeEngineManager();
            //engine.RegisterPlugin(".");
            //var test_cases = new string[] { "test-1", "test-100", "test-250" };
            //for (var ndx = 0; ndx < 2; ndx++)
            //{
            //    for (var _ = 0; _ < 2; _++)
            //    {
            //        logger.LogInfo($"test, {test_cases[ndx]}");
            //        engine.AddFiles(
            //        paths: new string[] { test_cases[ndx] },
            //        processAsync: false,
            //        addFileAsync: false);
            //        GC.Collect();
            //        Thread.Sleep(2000);
            //        engine.Clear();
            //        logger.LogInfo($"test, {test_cases[ndx]}, completed");
            //    }
            //}
            //logger.LogInfo("-- exit --");
            //Console.ReadLine();

            //
            //if (args.Length == 0)
            //{
            //    logger.LogError("Please run one of the commands");
            //    return;
            //}

            //switch (args[0])
            //{
            //    case "--create-files":
            //        var totalFiles = Convert.ToInt32(args[1]);
            //        break;
            //    case "--test":                    
            //        break;
            //}
        }
    }

    class CmdParser
    {
        List<CmdInfo> _info = new List<CmdInfo>();
        void AddCommand(string Cmd, string CmdHelper)
        {
            _info.Add(new CmdInfo() { Cmd = "--help", CmdHelper = "Print gmail" });
        }

        void Parse(string[] argc)
        {
            int ndx = 0;
            while (ndx < argc.Length)
            {
                if (argc[ndx].StartsWith("--"))
                {

                }
                else if (argc[ndx].StartsWith("-"))
                {
                }
                else
                {
                    ndx++;
                }

            }
        }
    }


    class CmdInfo
    {
        public string Cmd { get; set; }
        public string CmdHelper { get; set; }
    }



}
