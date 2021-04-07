using logAxeEngine.Common;
using logAxeEngine.EventMessages;

namespace logAxeEngine.Interfaces
{
   public interface ILogEngine : ILogLinesStorage
   {
      void RegisterPlugin(string folderPath);
      void AddFiles(string[] paths, bool processAsync = true, bool addFileAsync = true);
      LogFileInfo[] GetAllFileNames();
      void ExportFiles(LogFileInfo[] fileList, string exportFilePath);
      string GetLicenseInfo();

      IMessageBroker MessageBroker { get; }

      FileParseProgressEvent GetStartInfo();
   }
}
