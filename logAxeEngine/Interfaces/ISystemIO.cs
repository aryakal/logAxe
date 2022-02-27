//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeEngine.Interfaces
{
   /// <summary>
   /// This is wrapper class was created so that we can use the interface to test the engine.
   /// </summary>
   public interface ISystemIO
   {
      bool IsFile(string path);
      bool IsDirectory(string path);
      long GetFileSize(string path);
      string[] GetFilesInDir(string path);
      byte[] GetData(string path);
   }
}
