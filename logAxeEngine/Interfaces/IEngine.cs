//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Common;
using logAxeEngine.EventMessages;

namespace logAxeEngine.Interfaces
{
   public interface ILogEngine : ILogLinesStorage
   {
      IMessageBroker MessageBroker { get; }
      void RegisterPlugin(string folderPath);
      //void RegisterPlugin(string folderPath);
      void AddFiles(string[] paths, bool processAsync = true, bool addFileAsync = true);
      void AddFiles(IFileObject[] paths, bool processAsync = true, bool addFileAsync = true);
      LogFileInfo[] GetAllFileNames();
      void ExportFiles(LogFileInfo[] fileList, string exportFilePath);
      string GetLicenseInfo();      
      FileParseProgressEvent GetStartInfo();
   }
}
