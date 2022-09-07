//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
// TODO : Need to close the zip file when the files are cleaned.
//
//=====================================================================================================================

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

using logAxeCommon;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using logAxeEngine.Storage;
using logAxeEngine.EventMessages;
using logAxeCommon.Interfaces;
using logAxeCommon.Files;
using libALogger;
using libACommunication;

namespace logAxeEngine.Engines
{
   public class LogAxeEngineManager : ILogEngine
   {
      private int UniqueFileId { get; set; } = LogLine.INVALID;
      private readonly ILibALogger _logger;      
      private readonly FileParseProgressEvent _fileProgressStat = new FileParseProgressEvent();
      private readonly SemaphoreSlim _lockAddition = new SemaphoreSlim(1, 1);
      private readonly IPluginManager _pluginManger;
      private IMessageExchanger _messgeExchanger;

      private readonly List<IFileObject> _allFiles = new List<IFileObject>();
      private long _totalFileSize = 0;

      // Storage dbs
      private IStorageString _storeMsgStack;
      private IStorageString _storeTag;
      private IStorageDataBase _database;

      public LogAxeEngineManager(IPluginManager pluginManager)
      {
         _messgeExchanger = null;
         _logger = Logging.GetLogger("logEng");         

         _pluginManger = pluginManager;
         Clear();
      }

      #region Implements ILogLinesStorage
      public int TotalLogLines => _database.TotalLogLines;
      public void Clear()
      {
         if (null != _database)
         {
            _storeMsgStack.Clear();
            _storeTag.Clear();
            _database.Clear();

            _storeMsgStack = null;
            _storeTag = null;
            _database = null;

         }
         _fileProgressStat.TotalFileCount = 0;
         _fileProgressStat.TotalFileLoadedCount = 0;
         _fileProgressStat.TotalFileParsedCount = 0;
         _fileProgressStat.TotalFileRejectedCount = 0;
         _fileProgressStat.TotalFileSizeLoaded = 0;

         _storeMsgStack = new StorageStringDB();
         _storeTag = new StorageStringDB();
         _database = new StorageMetaDatabase(_storeMsgStack, _storeTag);
         _allFiles.Clear();
         _messgeExchanger?.BroadCast(new UnitMsg(opCode: WebFrameWork.CMD_PUT_NEW_VIEW, name: WebFrameWork.CLIENT_BST_ALL));
         UniqueFileId = LogLine.INVALID;
         GC.Collect();
      }
      public LogLine GetLogLine(int globalLineNumber)
      {
         var line = _database.GetLogLine(globalLineNumber);
         line.GlobalLine = globalLineNumber;
         line.Msg = line.MsgId != LogLine.INVALID ? _storeMsgStack.RetriveString(line.MsgId) : "";
         line.StackTrace = line.StackTraceId != LogLine.INVALID ? _storeMsgStack.RetriveString(line.StackTraceId) : "";
         if (line.TagId != LogLine.INVALID)
         {
            line.Tag = _storeTag.RetriveString(line.TagId);
            var words = line.Tag.Split(new[] { ";" }, 4, StringSplitOptions.None);
            line.ProcessId = int.Parse(words[0].Substring(2));
            line.ThreadNo = int.Parse(words[1].Substring(2));
            line.Category = words[2].Substring(2);
         }

         return line;
      }
      public LogType GetLogLineType(int globalLineNumber)
      {
         return _database.GetLogLineType(globalLineNumber);
      }
      public LogFrame Filter(TermFilter term)
      {
         var frame = _database.Filter(term);
         frame.TotalLogFiles = _allFiles.Count;
         return frame;
      }
      public LogFrame GetMasterFrame()
      {
         var frame = _database.GetMasterFrame();
         frame.TotalLogFiles = _allFiles.Count;
         return frame;
      }
      #endregion

      #region Implements ILogEngine      
      public void RegisterPlugin(string folderPath = @".")
      {
         _pluginManger.LoadPlugin(folderPath);
      }
      public void AddFiles(string[] paths, bool processAsync = true, bool addFileAsync = true)
      {
         AddFiles(HelperCovertDirZipFileToInternalFileObject(paths), processAsync, addFileAsync);
      }
      public void AddFiles(IFileObject[] paths, bool processAsync = true, bool addFileAsync = true)
      {
         var tsk = Task.Run(() =>
         {
            ProcessFiles(paths, processAsync);
         });

         if (!addFileAsync)
         {
            tsk.Wait();
         }
      }

      public LogFileInfo[] GetAllLogFileInfo()
      {
         var lst = new List<LogFileInfo>();
         foreach (var at in _allFiles)
         {
            lst.Add(new LogFileInfo() { 
               DisplayName = at.FileName, 
               FileNo = at.InfoTracker.UniqueFileNo, 
               Key = "", 
               IsLoaded= at.InfoTracker.IsProcessed, 
               ParserName=at.InfoTracker.LogParser == null ? "" : at.InfoTracker.LogParser.ParserName
            });
         }
         return lst.ToArray();
      }

      public void ExportFiles(LogFileInfo[] fileList, string exportFilePath)
      {
         //var currentNo = 0;
         using (FileStream zipToOpen = new FileStream(
             exportFilePath,
             FileMode.OpenOrCreate))
         {
            using (var archive = new ZipFileCreator(zipToOpen))
            {
               foreach (var fileKey in fileList)
               {
                  try
                  {
                     if (_allFiles.Any(x => x.InfoTracker.UniqueFileNo == fileKey.FileNo))
                     {
                        //TODO need to propage the error.
                        var fo = _allFiles.First(x => x.InfoTracker.UniqueFileNo == fileKey.FileNo);
                        archive.AddEntry(fo.FileName, fo.GetFileData());
                        //currentNo++;
                        //_msgHelper.PostMessage(new FileExportProgressEvent()
                        //{
                        //   CurrentFileNo = currentNo,
                        //   TotalFiles = fileKeys.Length
                        //});
                     }
                  }
                  catch
                  {

                  }
               }
            }
            
         }
      }


      public string GetLicenseInfo()
      {
         return " The MIT License (MIT)" +
             "Copyright © 2022 Aryakal" +
             "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" +
             "The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." +
             "THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." +
             _pluginManger.GetAllPluginsInfo();
      }
      public FileParseProgressEvent GetStartInfo()
      {
         return _fileProgressStat;
      }

      #endregion

      #region Private Functions

      /// <summary>
      /// This will expand the zip file or folder or file and add as an file.
      /// </summary>
      /// <param name="filePaths"></param>
      /// <returns></returns>
      private IFileObject[] HelperCovertDirZipFileToInternalFileObject(string[] filePaths)
      {         
         var lst = new List<IFileObject>();
         foreach (var path in filePaths)
         {
            if (File.Exists(path))
            {
               try
               {
                  if (Path.GetExtension(path).ToLower() == ".zip")
                  {
                     var archive = new CompressionHelper(path);
                     foreach (var filePath in archive.GetAllFileName())
                     {
                        lst.Add(new CompressedFile(filePath, archive));
                     }
                  }
                  else
                  {

                     lst.Add(new RegularFile(path));
                  }
               }
               catch
               {
                  lst.Add(new BadFile(path));
               }
            }
            else if (Directory.Exists(path))
            {
               lst.AddRange(HelperCovertDirZipFileToInternalFileObject(Directory.GetFiles(path)));
            }
         }
         return lst.ToArray();
      }

      /// <summary>
      /// Function to guess the parser and create internal file object;
      /// </summary>
      /// <param name="files"></param>
      /// <returns></returns>
      private Tuple<int, long> HelperAssociateParser(IFileObject[] files) {         
         long totalFileSize = 0;
         int validFiles = 0;
         foreach (var fo in files)
         {
            fo.InfoTracker = new FileTrackerInfo()
            {
               UniqueFileNo = ++UniqueFileId,
               LogParser = fo.IsFileValid ? _pluginManger.GuessParser(fo.FileName) : null
            };

            if (null == fo.InfoTracker.LogParser)
               continue;
            
            validFiles += 1;
            totalFileSize += fo.FileSize;
         }
         return Tuple.Create(validFiles, totalFileSize);
      }


      private void ProcessFiles(IFileObject[] files, bool useParallelTasks = true)
      {
         var (validFiles, totalFileSize) = HelperAssociateParser(files);
         _totalFileSize += totalFileSize;
         _allFiles.AddRange(files);

         if (0 == validFiles)
            return;
         

         _fileProgressStat.TotalFileCount += validFiles;
         _fileProgressStat.ParseComplete = false;

         if (useParallelTasks)
         {
            Parallel.For(0, files.Length, ndx =>
            {
               AddFileToIndex(files[ndx]);
            });
         }
         else
         {
            for (int ndx = 0; ndx < files.Length; ndx++)
            {
               AddFileToIndex(files[ndx]);
            }
         }
         
         _database.OptimizeData();
         //_messenger.PostMessage(LogAxeMessageEnum.EngineOptmizeComplete);
         //_messgeExchanger?.BroadCast(null);
         Utils.ClearAllGCMemory();
         GC.Collect();
         _logger.Debug("optmizing data compeleted");         
         _fileProgressStat.ParseComplete = true;
         //_messenger.PostMessage(_fileProgressStat);
         //_messgeExchanger?.BroadCast(null);
         //_messenger.PostMessage(LogAxeMessageEnum.NewViewAnnouncement);
         _messgeExchanger?.BroadCast(new UnitMsg(opCode: WebFrameWork.CMD_PUT_NEW_VIEW, name: WebFrameWork.CLIENT_BST_ALL));         
      }
      private void AddFileToIndex(IFileObject fo)
      {
         if (fo.InfoTracker.IsProcessed || fo.InfoTracker.LogParser == null)
         {
            _lockAddition.Wait();
            _fileProgressStat.TotalFileSizeLoaded += fo.FileSize;
            _fileProgressStat.TotalFileLoadedCount++;
            _fileProgressStat.TotalFileParsedCount++;
            _lockAddition.Release();
            return;
         }

         var logFile = new LogFile(
            Path.GetFileName(fo.FileName),
            fo.GetFileData()
            )
         {
            FileId = fo.InfoTracker.UniqueFileNo
         };


         try
         {
            fo.InfoTracker.LogParser.ParseFile(logFile);
         }
         catch (Exception ex)
         {
            fo.InfoTracker.ExceptionDetails = ex;            
         }


         _lockAddition.Wait();
         try
         {
            if (fo.InfoTracker.HasException)
            {
               _fileProgressStat.TotalFileLoadedCount++;
               _fileProgressStat.TotalFileRejectedCount++;
               fo.InfoTracker.IsProcessed = false;
            }
            else 
            {
               _fileProgressStat.TotalFileSizeLoaded += logFile.FileData.Length;
               _fileProgressStat.TotalFileLoadedCount++;
               _database.AddLogFile(logFile, logFile.FileId);
               _fileProgressStat.TotalFileParsedCount++;
               fo.InfoTracker.IsProcessed = true;               
            }
            //_messenger.PostMessage(_fileProgressStat);

         }
         catch
         {
         }
         finally
         {
            _lockAddition.Release();
         }

         logFile.Clear();
      }

      public void RegisterMessageExchanger(IMessageExchanger exchanger)
      {
         _messgeExchanger = exchanger;
      }

      public UnitCmdFileAppMemInfo GetFileAppMemInfo()
      {
         return new UnitCmdFileAppMemInfo() { AppSize = new AppSize().Memory, FileSize = _totalFileSize };
      }

      #endregion
   }
}
