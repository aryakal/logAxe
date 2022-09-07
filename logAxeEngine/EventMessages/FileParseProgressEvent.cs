﻿//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;
namespace logAxeEngine.EventMessages
{
   public class FileParseProgressEvent
   {
      public string FromClientID { get; set; }
      public long TotalFileCount { get; set; }
      public long TotalFileLoadedCount { get; set; }
      public long TotalFileParsedCount { get; set; }
      public long TotalFileRejectedCount { get; set; }
      public long TotalFileSizeLoaded { get; set; }
      public bool OptimizingIndexingData { get; set; }
      public bool ParseComplete { get; set; }
   }
}
