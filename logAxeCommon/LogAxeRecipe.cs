//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

namespace logAxeCommon
{
   public class LogAxeRecipe
   {
      public string[] FilePaths { get; set; }
      public LogAxeRecipeAction[] Actions { get; set; }
      public bool ProcessAsync { get; set; } = true;      
   }

   public class LogAxeRecipeAction { 
      public TermFilter Filter { get; set; } 
      public string ExportTxtFile { get; set; }
      public string ExportJsonFile { get; set; }
      public string ExportFilePathFile { get; set; }
      public string ExportSourceFilePath { get; set; }
      public bool ExportPerfomanceParameters { get; set; } = false;
   }
}
