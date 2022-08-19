//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace logAxeCommon
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

      public void SetEnabledValue(string cmdName, bool value)
      {
         _cmds[cmdName].IsSet = value;
      }

      public string GetString(string cmdName)
      {
         return (string)GetData(cmdName);
      }

      public int GetInt(string cmdName)
      {
         return (int)GetData(cmdName);
      }

      private object GetData(string cmdName)
      {
         return _cmds[cmdName].Value == null ? _cmds[cmdName].DefaultValue : _cmds[cmdName].Value;
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
                  if (ndx >= argc.Length || argc[ndx].StartsWith("--"))
                  {
                     if (cmd.EnableUseDefault)
                     {
                        continue;
                     }

                     Console.WriteLine($"\nerror, cmd [{cmd.Cmd}] expected a value\n");
                     printHelp = true;
                     break;

                  }
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
               if (_cmds[cmd].ValueType == typeof(bool))
                  Console.WriteLine($"{cmd.PadRight(15)}       {_cmds[cmd].CmdHelper}");
               else
                  Console.WriteLine($"{cmd.PadRight(15)} <val> {_cmds[cmd].CmdHelper}");

            }
         }
      }
   }


   public class CmdInfo
   {
      public string Cmd { get; set; }
      public string CmdHelper { get; set; }
      public bool EnableUseDefault { get; set; }
      public bool IsBoolean => (ValueType == typeof(bool));
      public object Value { get; set; }
      public Type ValueType { get; set; }
      public object DefaultValue { get; set; }
      public bool IsSet { get; set; }
   }
}
