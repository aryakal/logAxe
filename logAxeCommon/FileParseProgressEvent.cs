//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================


namespace logAxeCommon
{
   public class FileParseProgressEvent
   {
      public string FromClientID { get; set; }
      public int TotalFile { get; set; }
      public int FilesParsed { get; set; }
      public int FilesIndexed { get; set; }
      public int FilesRejected { get; set; }
      public bool FilesOptimizing { get; set; }
      public long FilesLoadedSize { get; set; }      
      public bool ParseInProgress { get; set; }      
      public long AppSize { get; set; }
      public override string ToString()
      {
         return $"TotalFile: {TotalFile}, FilesParsed: {FilesParsed}, FilesIndexed: {FilesIndexed}, ParseInProgress: {ParseInProgress}";
      }

      public void Reset() {
         TotalFile = 0;
         FilesParsed = 0;
         FilesIndexed = 0;
         FilesRejected = 0;
         FilesLoadedSize = 0;
      }
   }
}
