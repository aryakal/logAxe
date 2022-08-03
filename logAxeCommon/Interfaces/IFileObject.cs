//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeCommon.Interfaces
{
   public interface IFileObject {
      byte[] GetFileData ();
      long FileSize { get; }
      string FileName { get;}
      bool IsFileValid { get; }
      FileTrackerInfo InfoTracker { get; set; }
   }
}
