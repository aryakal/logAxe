using System.Collections.Generic;
using logAxeEngine.Interfaces;
using System.IO;
using System.IO.Compression;

namespace logAxeEngine.Common
{
   public class FileObjectHelper
   {
      ISystemIO _io;
      public FileObjectHelper(ISystemIO io)
      {
         _io = io;
      }
      public List<FileObject> expand_compresssed_files_and_files(string[] paths)
      {
         var lst = new List<FileObject>();
         foreach (var path in paths)
         {
            if (_io.IsDirectory(path))
            {
               lst.AddRange(expand_compresssed_files_and_files(_io.GetFilesInDir(path)));
            }
            else if (_io.IsFile(path))
            {
               if (Path.GetExtension(path).ToLower() == ".zip")
               {
                  var archive = ZipFile.OpenRead(path);
                  foreach (var entry in archive.Entries)
                  {
                     if (entry.FullName.EndsWith("/"))
                        continue;

                     lst.Add(new FileObject()
                     {
                        Archive = archive,
                        FileName = Path.GetFileName(entry.FullName),
                        FilePath = entry.FullName,
                        FileSize = entry.Length                        
                        
                     });
                  }
               }
               else
               {
                  lst.Add(new FileObject()
                  {
                     FileName = Path.GetFileName(path),
                     FilePath = path,
                     FileSize = _io.GetFileSize(path)
                  }); ;
               }
            }

         }
         return lst;
      }
      public byte[] GetFileData(FileObject fileObj)
      {
         byte[] data = null;

         if (fileObj.IsZipFile)
         {
            using (var stream = fileObj.Archive.GetEntry(fileObj.FilePath).Open())
            {
               data = new byte[fileObj.FileSize];
               stream.Read(data, 0, (int)fileObj.FileSize);
               stream.Close();
            }
         }
         else
         {
            data = _io.GetData(fileObj.FilePath);
         }
         return data;
      }
   }
}
