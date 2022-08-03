//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeCommon
{
   public class PluginFeatureSupport
   {
      public enum SupportType { 
         None,
         Yes,
         No      
      }
      public SupportType ProcID { get; set; } = SupportType.None;
      public SupportType ThreadID { get; set; } = SupportType.None;
      public SupportType Tag { get; set; } = SupportType.None;
   }
}
