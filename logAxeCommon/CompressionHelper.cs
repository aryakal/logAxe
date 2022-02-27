//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.IO;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.Zip;


namespace logAxeCommon
{
   public class CompressionHelper
   {
      
      private readonly ZipFile _zipArchive;

      public CompressionHelper(string filePath)
      {      
         _zipArchive = new ZipFile(new FileStream(filePath, FileMode.Open, FileAccess.Read));
      }

      public byte[] ReadFile(string filePath)
      {
         var entry = _zipArchive.GetEntry(filePath);
         using (var s = _zipArchive.GetInputStream(entry))
         {
            var bt = new byte[entry.Size];
            if (s.Read(bt, 0, bt.Length) != entry.Size)
            {
               throw new Exception("Error in reading file.");
            }
            return bt;
         }
      }

      public long GetFileSize(string filePath)
      {
         return _zipArchive.GetEntry(filePath).Size;
      }

      public string[] GetAllFileName() { 
         var lst = new List<string>();
         foreach (ZipEntry entry in _zipArchive)
         {
            if (entry.IsFile)
            {
               lst.Add(entry.Name);
            }
         }

         return lst.ToArray();
      }
   }

   public class ZipFileCreator : IDisposable
   {      
      ZipOutputStream _outStream;
      public ZipFileCreator(FileStream stream)
      {
         //_zipFile = new FileStream(filePath, FileMode.OpenOrCreate);
         _outStream = new ZipOutputStream(stream);
         _outStream.SetLevel(1);

      }

      public void AddEntry(string fileName, byte [] data)
      {
         var entry = new ZipEntry(Path.GetFileName(fileName));
         _outStream.PutNextEntry(entry);
         _outStream.Write(data, 0, data.Length);
      }

      public void Dispose()
      {
         _outStream.Close();
      }
   }
}
