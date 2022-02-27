//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.IO;
using logAxeEngine.Interfaces;

namespace logAxeEngine.Common
{
   /// <summary>
   /// Interface used for testing so that we need not create files on the disk.
   /// </summary>
   public sealed class SystemIO : ISystemIO
   {
      public byte[] GetData(string path)
      {
         return File.ReadAllBytes(path);
      }

      public string[] GetFilesInDir(string path)
      {
         return Directory.GetFiles(path);
      }

      public long GetFileSize(string path)
      {
         return new FileInfo(path).Length;
      }

      public bool IsDirectory(string path)
      {
         return Directory.Exists(path);
      }

      public bool IsFile(string path)
      {
         return File.Exists(path);
      }
   }
}
