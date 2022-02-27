//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using Newtonsoft.Json;

namespace logAxeEngine.Common
{
   public class LogAxeConfig
   {
      [JsonProperty("Color")]
      public LogAxeColorConfig ColorConfig { get; set; } = new LogAxeColorConfig();
      [JsonProperty("Font")]
      public LogAxeColorConfig FontConfig { get; set; } = new LogAxeColorConfig();
   }

   public class LogAxeColorConfig {
      public string Background { get; set; } = "#f5f5f5";
      public string Error { get; set; } = "red";
      public string Info { get; set; } = "black";
      public string Trace { get; set; } = "green";
      public string Warning { get; set; } = "#8E4C3E";
      public string Text { get; set; } = "black";
      public string SideBarText { get; set; } = "black";
   }

   public class LogAxeFontConfig
   {
      public string SideBar { get; set; } = "normal 13px \"Segoe UI\"";
      public string SideBarHeading { get; set; } = "normal 13px \"Segoe UI\"";
   }
}
