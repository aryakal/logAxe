//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon.Interfaces;
using System.IO;


namespace logAxeCommon.Files
{
   public class CompressedFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly string _filePath;
      readonly CompressionHelper _archive;

      public CompressedFile(string filePath, CompressionHelper zipArchive)
      {
         _filePath = filePath;
         FileName = Path.GetFileName(filePath);
         _archive = zipArchive;
      }

      public string FileName { get; private set; }
      public long FileSize { get { return _archive.GetFileSize(_filePath); } }

      public byte[] GetFileData()
      {
         return _archive.ReadFile(_filePath);
      }
   }
}
