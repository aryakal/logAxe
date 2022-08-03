//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libACommunication;


namespace logAxeCommon.Interfaces
{
   public interface IMessageExchanger
   {      
      void BroadCast(UnitCmd cmd);
   }
}
