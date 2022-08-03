//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon.Interfaces;
using System.IO;

namespace logAxeCommon.Files
{
   public class RegularFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly string _filePath;
      public RegularFile(string filePath)
      {
         _filePath = filePath;
         FileName = Path.GetFileName(filePath);
         FileSize = new FileInfo(_filePath).Length;
      }
      public string FileName { get; private set; }
      public long FileSize { get; private set; }
      public byte[] GetFileData()
      {
         return File.ReadAllBytes(_filePath);
      }
   }
}
