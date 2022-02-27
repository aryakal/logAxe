//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace logAxeEngine
{
   public class CmdParser
   {
      List<CmdInfo> _info = new List<CmdInfo>();
      Dictionary<string, CmdInfo> _cmds = new Dictionary<string, CmdInfo>();
      public bool Proceed { get; set; }
      public CmdParser()
      {
         _info.Add(new CmdInfo() { Cmd = "--help", CmdHelper = "Prints this help message" });
      }
      public void AddCommand(CmdInfo info)
      {
         _cmds[info.Cmd] = info;
      }
      public bool IsEnabled(string cmdName)
      {
         return _cmds[cmdName].IsSet;
      }

      public string GetString(string cmdName)
      {
         return (string)_cmds[cmdName].Value;
      }

      public int GetInt(string cmdName)
      {
         return (int)_cmds[cmdName].Value;
      }

      public void Parse(string[] argc)
      {
         var printHelp = argc.Length == 0;
         int ndx = 0;
         while (ndx < argc.Length)
         {
            if (argc[ndx].StartsWith("--"))
            {
               if (!_cmds.ContainsKey(argc[ndx]))
               {
                  printHelp = true;
                  break;
               }
               var cmd = _cmds[argc[ndx]];
               cmd.IsSet = true;
               if (!cmd.IsBoolean)
               {
                  ndx++;
                  cmd.Value = Convert.ChangeType(argc[ndx], cmd.ValueType);
               }
               ndx++;
            }
            else
            {
               printHelp = true;
               break;
            }

         }

         Proceed = !printHelp;

         if (printHelp)
         {
            var lst = _cmds.Keys.ToArray();
            Array.Sort(lst);
            foreach (var cmd in lst)
            {
               Console.WriteLine($"{cmd.PadRight(20)} {_cmds[cmd].CmdHelper}");
            }
         }
      }
   }


   public class CmdInfo
   {
      public string Cmd { get; set; }
      public string CmdHelper { get; set; }
      public bool IsBoolean => (ValueType == typeof(bool));
      public object Value { get; set; }
      public Type ValueType { get; set; }
      public object DefaultValue { get; set; }
      public bool IsSet { get; set; }
   }
}
