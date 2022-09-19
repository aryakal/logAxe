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

   public class UnitCmdFileAppMemInfo
   {
      public long FileSize { get; set; }
      public long AppSize { get; set; }
   }

   public class WebFrameWork {
      public const string CMD_PUT_INFO = "cmd_put_info";
      public const string CMD_PUT_FILES = "cmd_put_files";
      public const string CMD_GET_LINES = "cmd_get_lines";
      public const string CMD_PUT_LINES = "cmd_put_lines";
      public const string CMD_PUT_REGISTER = "cmd_put_register";
      public const string CMD_PUT_UNREGISTER = "cmd_put_unregister";
      public const string CMD_PUT_NEW_VIEW = "cmd_put_new_view";      
      public const string CLIENT_BST_ALL = "client_bst_all";      
      
      public const string CMD_PUT_CLEAR = "cmd_put_clear";

      public const string CMD_GET_CONFIG_CURRENT = "cmd_get_config_current";
      public const string CMD_PUT_CONFIG_CURRENT = "cmd_put_config_current";
      public const string CMD_GET_CONFIG_LIST = "cmd_get_config_list";
      public const string CMD_POST_CONFIG = "cmd_post_config";

      //public const string CMD_GET_FILTER_THEME_INFO = "cmd_get_filter_theme_info";
      //public const string CMD_PUT_FILTER_THEME_INFO = "cmd_put_filter_theme_info";

      public const string CMD_GET_FILTER_LIST = "cmd_get_filter_list";      
      public const string CMD_POST_FILTER_SAVE = "cmd_post_filter_save";
      public const string CMD_DEL_FILTER_DETAIL = "cmd_del_filter_detail";
      public const string CMD_SET_FILTER = "cmd_set_filter";
      public const string CMD_GET_FILTER_DETAILS = "cmd_get_filter_details";
      public const string CMD_PUT_FILTER_DETAILS = "cmd_put_filter_details";

      

      public const string CMD_GET_CLIENT_MSG = "cmd_get_client_msg";
      public const string CMD_PUT_CLIENT_BST = "cmd_put_client_bst";

      public const string CMD_GET_FILE_LIST = "cmd_get_file_list";
      public const string CMD_PUT_FILE_LIST = "cmd_put_file_list";

      public const string CMD_GET_EXPORT_FILES = "cmd_get_export_files";

      //public const string CMD_GET_FILE_APP_MEM_INFO = "cmd_get_file_app_mem_info";
      //public const string CMD_PUT_FILE_APP_MEM_INFO = "cmd_put_file_app_mem_info";

      public const string CMD_PUT_ALL_VIEW_UPDATE = "cmd_put_all_view_update";
      public const string CMD_PUT_ALL_FILTER_UPDATE = "cmd_put_all_filter_update";
      public const string CMD_PUT_ALL_THEME_UPDATE = "cmd_put_all_theme_update";

      public const string CMD_MSG_PROGRESS = "cmd_msg_progress";
      public const string MSG_BST_PROGRESS = "msg_bst_progress";
      public const string MSG_GLOBAL_LINE = "msg_global_line";
      public const string MSG_NAVIGATE_TO_VIEW_LINE = "msg_navigate_to_view_line";
      public const string MSG_COPY_TO_CLIPBOARD = "msg_copy_to_clipboard";
      public const string MSG_COPY_TO_CLIPBOARD_UNTIL_FIXED_HTML = "msg_copy_to_clipboard_until_fixed_html";
      public const string MSG_COPY_TO_CLIPBOARD_UNTIL_FIXED_PLAIN = "msg_copy_to_clipboard_until_fixed_plain";
   }
}

