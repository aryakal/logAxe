//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace libALogger
{
   public interface ILibALogger
   {
      void Info(string message);
      void Debug(string message);
      void Error(string message);
   }

   public interface ILibAHandler : ILibALogger
   {
      //void OpenStream();
      //void CloseStream();
   }

}
