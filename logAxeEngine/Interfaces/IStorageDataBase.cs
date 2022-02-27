//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Common;

namespace logAxeEngine.Interfaces
{
   public interface IStorageDataBase : ILogLinesStorage
   {
      void AddLogFile(LogFile logFile, int fileNumber);
      void OptimizeData();
   }
}
