//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon.Interfaces;
using System.IO;


namespace logAxeCommon.Files
{
   public class WebFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly byte[] _data;
      public WebFile(string fileName, byte[] data)
      {
         FileName = Path.GetFileName(fileName);
         _data = data;
         FileSize = _data.Length;
      }
      public string FileName { get; private set; }
      public long FileSize { get; private set; }
      public byte[] GetFileData()
      {
         return _data;
      }
   }
}
