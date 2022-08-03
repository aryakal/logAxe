//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;

using logAxeCommon;
using logAxeCommon.Interfaces;

namespace logAxeCommon
{
   public class FileTrackerInfo
   {
      public ILogParser LogParser { get; set; }
      public bool IsProcessed { get; set; }
      public Exception ExceptionDetails { get; set; } = null;
      public bool HasException { get { return ExceptionDetails != null; } }  
      public int UniqueFileNo { get; set; }

      public static implicit operator List<object>(FileTrackerInfo v)
      {
         throw new NotImplementedException();
      }
   }
}
