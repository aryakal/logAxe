using logAxeCommon;
using System.IO.Compression;

namespace logAxeEngine.Common
{
   public sealed class FileObject
   {
      public int UniqueFileNo { get; set; } = LogLine.INVALID;
      public string FileName { get; set; }
      public string FilePath { get; set; }
      public long FileSize { get; set; }
      public ILogParser LogParser { get; set; }
      public bool IsZipFile => ZipArchivePath != "";
      public string ZipArchivePath { get; set; } = "";
   }
}
