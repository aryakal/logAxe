//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon.Interfaces;
using System;

namespace logAxeCommon.Files
{
   /// <summary>
   /// If for some reason there is an issue when loading, extracting file then it will be set has bad file.
   /// </summary>
   public class BadFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = false;
      public BadFile(string filePath)
      {
         FileName = filePath;
      }
      public string FileName { get; private set; }
      public long FileSize { get; } = 0;
      public byte[] GetFileData()
      {
         throw new NotImplementedException();
      }
   }
}
