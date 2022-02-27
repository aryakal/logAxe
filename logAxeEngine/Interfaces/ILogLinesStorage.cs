//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;
using logAxeEngine.Common;

namespace logAxeEngine.Interfaces
{
   public interface ILogLinesStorage
   {
      int TotalLogLines { get; }
      void Clear();

      LogLine GetLogLine(int globalLineNumber);
      LogType GetLogLineType(int globalLineNumber);

      LogFrame Filter(TermFilter term);
      LogFrame GetMasterFrame();
   }

}
