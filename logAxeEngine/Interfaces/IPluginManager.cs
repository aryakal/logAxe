//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;

namespace logAxeEngine.Interfaces
{
   public interface IPluginManager
   {
      ILogParser GuessParser(string fileName);
      string GetAllPluginsInfo();

      void LoadPlugin(string directoryPath);
      void LoadPlugin(ILogParser parser);
   }
}
