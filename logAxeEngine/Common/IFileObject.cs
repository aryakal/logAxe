//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;
using System;
using System.Collections.Generic;
using System.IO;

namespace logAxeEngine.Common
{
   public interface IFileObject {
      byte[] GetFileData ();
      long FileSize { get; }
      string FileName { get;}
      bool IsFileValid { get; }
      FileTrackerInfo InfoTracker { get; set; }
   }

   public class FileTrackerInfo
   {
      public ILogParser LogParser { get; set; }
      public bool IsProcessed { get; set; }
      public int UniqueFileNo { get; set; }

      public static implicit operator List<object>(FileTrackerInfo v)
      {
         throw new NotImplementedException();
      }
   }

   public class RegularFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly string _filePath;
      public RegularFile(string filePath)
      {
         _filePath = filePath;
         FileName = Path.GetFileName(filePath);
         FileSize = new FileInfo(_filePath).Length;
      }      
      public string FileName { get; private set; } 
      public long FileSize { get; private set; }
      public byte[] GetFileData()
      {
         return File.ReadAllBytes(_filePath);
      }
   }

   public class WebFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly byte[] _data;
      public WebFile(string fileName, byte [] data)
      {  
         FileName = Path.GetFileName(fileName);
         _data = data;
         FileSize = _data.Length;
      }
      public string FileName { get; private set; }
      public long FileSize { get; private set; }
      public byte[] GetFileData()
      {
         return _data;
      }
   }

   public class CompressedFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = true;
      readonly string _filePath;
      readonly CompressionHelper _archive;

      public CompressedFile(string filePath, CompressionHelper zipArchive)
      {
         _filePath = filePath;
         FileName = Path.GetFileName(filePath);      
         _archive = zipArchive;
      }

      public string FileName { get; private set; }
      public long FileSize { get { return _archive.GetFileSize(_filePath); } }

      public byte[] GetFileData()
      {
         return _archive.ReadFile(_filePath);
      }
   }

   public class BadFile : IFileObject
   {
      public FileTrackerInfo InfoTracker { get; set; }
      public bool IsFileValid { get; } = false;      
      public BadFile(string filePath)
      {
         FileName = filePath;         
      }
      public string FileName { get; private set; }
      public long FileSize { get; } = 0;
      public byte[] GetFileData()
      {
         throw new NotImplementedException();
      }
   }
}
