//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using Newtonsoft.Json;
using System.Collections.Generic;

namespace logAxeCommon
{
   
   public enum LogAxeUnitCommands { 
      Register,
      ClientInfo,
      DataLines,
   }

   public interface ILogAxeUnitCommand
   {
      LogAxeUnitCommands Command { get; }
   }

   public class RegisterClient : ILogAxeUnitCommand
   {
      [JsonIgnore]
      public LogAxeUnitCommands Command => LogAxeUnitCommands.Register;

      public RegisterClient()
      {
       
      }
      public RegisterClient(string name)
      {
         Name = name;
      }
      public string Name { get; set; }
      public bool IsViewRequired { get; set; } = true;
   }

   public class UnitCmdAddDiskFiles {
      public string[] FilePaths { get; set; } = new string[0];
   }

   public class UnitCmdGetLines
   {
      [JsonProperty("s")]
      public int StartLine { get; set; }
      [JsonProperty("l")]
      public int Length { get; set; }
   }

   public class UnitCmdGetThemeFilterVersionInfo { 
      public string VersionInfo { get; set; }
      public string[] ListFilterNames { get; set; } = new string[0];
      public string[] ListThemeNames { get; set; } = new string[0];
      public ConfigUI CurrentConfigUI { get; set; } = new ConfigUI();
   }

   public class UnitCmdExportFile
   {
      
      public LogFileInfo[] Files { get; set; }
      public string ExportFileName { get; set; }
   }

   public class WebFrameWork {
      public const string CMD_SET_INFO = "cmd_set_info";
      public const string CMD_SET_FILES = "cmd_set_files";
      public const string CMD_GET_LINES = "cmd_get_lines";
      public const string CMD_SET_LINES = "cmd_set_lines";
      public const string CMD_SET_REGISTER = "register";
      public const string CMD_BST_NEW_VIEW = "cmd_bc_new_view";
      public const string CMD_SET_NEW_VIEW = "cmd_set_new_view";
      public const string CLIENT_BST_ALL = "all-clients";      
      
      public const string CMD_SET_CLEAR = "cmd_set_clear";

      public const string CMD_GET_FILTER_THEME_INFO = "cmd_get_filter_theme_info";
      public const string CMD_SET_FILTER_THEME_INFO = "cmd_set_filter_theme_info";

      public const string CMD_GET_CONFIG_CURRENT = "cmd_get_config_current";
      public const string CMD_LIST_CONFIGS = "cmd_get_config_all";
      public const string CMD_SAVE_CONFIG = "cmd_save_config";

      public const string CMD_LIST_FILTERS = "cmd_get_filter_all";
      public const string CMD_SAVE_FILTER = "cmd_save_fitler";
      public const string CMD_SET_FILTER = "cmd_set_filter";
      public const string CMD_GET_INFO_FILTER = "cmd1";
      public const string CMD_SET_INFO_FILTER = "cmd2";

      public const string CMD_BST_NEW_THEME = "cmd_boradcast_new_theme";

      public const string CMD_CLIENT_MSG = "cmd_client_message";
      public const string CMD_CLIENT_BST = "cmd_client_broadcast";

      public const string CMD_GET_FILE_LIST = "cmd_get_file_list";
      public const string CMD_SET_FILE_LIST = "cmd_set_file_list";

      public const string CMD_EXPORT_FILES = "cmd_export_file_list";
   }
}

