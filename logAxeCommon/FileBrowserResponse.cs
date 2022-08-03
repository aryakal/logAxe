//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace logAxeCommon
{
   public class FileBrowserResponse
   {

      [JsonProperty("op")]
      public string Operation { get; set; }
      [JsonProperty("files")]
      public List<DirItem> FilePaths { get; set; } = new List<DirItem>();

      [JsonProperty("dirs")]
      public List<DirItem> DirPaths { get; set; } = new List<DirItem>();
   }
}
