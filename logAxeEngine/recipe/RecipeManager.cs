//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;
using logAxeEngine.Interfaces;
using System;
using System.Text;
using System.IO;
using libALogger;

namespace logAxeEngine.recipe
{
   public class RecipeManager
   {
      ILibALogger _logger;
      public RecipeManager(ILibALogger logger = null) {
         _logger = logger;
      }
      public void Execute(LogAxeRecipe recipe, ILogEngine engine) {
         _logger?.Info("Starting log churning recipe");
         engine.AddFiles(paths: recipe.FilePaths, processAsync: recipe.ProcessAsync, addFileAsync:false);

         _logger?.Debug($"Getting master frame");
         var masterFrame = engine.GetMasterFrame();

         _logger?.Debug($"Going to execute [{recipe.Actions.Length}] steps");
         foreach (var step in recipe.Actions) {

            var frame = engine.Filter(step.Filter);
            

            //if (step.ExportPerfomanceParameters) {
            //   _logger?.Trace($"Executing filter.");
            //}

            if (!string.IsNullOrEmpty(step.ExportTxtFile))
            {
               _logger?.Debug($"Exporting file {step.ExportTxtFile}");

               var selectedPLines = new StringBuilder();
               for (var ndx = 0; ndx < frame.TotalLogLines; ndx++)
               {    
                  selectedPLines.Append($"{Utils.ConvertLineToStr(engine.GetLogLine(frame.TranslateLine(ndx)))}{Environment.NewLine}");
               }
               File.WriteAllText(step.ExportTxtFile, selectedPLines.ToString());
            }

            //if (!string.IsNullOrEmpty(step.ExportJsonFile))
            //{
            //   //Write he file
            //}

            //if (!string.IsNullOrEmpty(step.ExportSourceFilePath))
            //{
            //   //Write he file
            //}
         }
      }
   }
}
