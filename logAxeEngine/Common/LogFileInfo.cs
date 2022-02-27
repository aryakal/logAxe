//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================


using Newtonsoft.Json;

namespace logAxeEngine.Common
{
   public class LogFileInfo
   {
      [JsonProperty("fno")]
      public int FileNo { get; set; }
      [JsonProperty("n")]
      public string DisplayName { get; set; }
      [JsonProperty("k")]
      public string Key { get; set; }
   }
}
