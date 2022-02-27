﻿//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace libWebServer
{
   public interface ISocketID
   {
      string ID { get; }
   }

   public class SimpleID : ISocketID
   {
      public SimpleID(string id)
      {
         ID = id;
      }
      public string ID { get; }
   }
}
