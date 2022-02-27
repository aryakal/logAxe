//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System.Diagnostics;

namespace logAxeEngine.Common
{
   public class AppSize
   {
      public AppSize()
      {
         Memory = Process.GetCurrentProcess().PrivateMemorySize64;
         Memorys = Utils.GetHumanSize(Memory);
      }
      public long Memory { get; private set; }
      public string Memorys { get; private set; }
      public override string ToString()
      {
         return Memorys;
      }
   }
}
