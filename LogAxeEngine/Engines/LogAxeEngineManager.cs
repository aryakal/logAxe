using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using logAxeCommon;
using logAxeEngine.Common;
using logAxeEngine.Storage;
using logAxeEngine.Interfaces;
using logAxeEngine.EventMessages;
using System.IO;
using System.Linq;
using System.IO.Compression;

namespace logAxeEngine.Engines
{
   public class LogAxeEngineManager : ILogEngine
   {
      private PluginManager _pluginManger = new PluginManager();
      private NamedLogger _logger = new NamedLogger("logEng");
      private IStorageString _storeMsgStack;
      private IStorageString _storeTag;
      private IStorageDataBase _database;
      private FileObjectHelper _ioHelper;
      private List<FileObject> _allFiles = new List<FileObject>();
      public IMessageBroker MessageBroker { get; }
      private int UniqueFileId { get; set; } = LogLine.INVALID;
      private MessageExchangeHelper _messenger;
      private FileParseProgressEvent _fileProgressStat;
      
      public LogAxeEngineManager(ISystemIO io = null)
      {
         _fileProgressStat = new FileParseProgressEvent();
         _ioHelper = new FileObjectHelper(io ?? new SystemIO());
         MessageBroker = new LogMessageEngine();
         MessageBroker.Start();
         _messenger = new MessageExchangeHelper(MessageBroker, null);
         Clear();
      }
      public int TotalLogLines => _database.TotalLogLines;
      public void RegisterPlugin(string folderPath = @".")
      {
         _pluginManger.LoadFromFolder(folderPath);
      }
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
         _messenger.PostMessage(LogAxeMessageEnum.NewViewAnnouncement);
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
         return _database.Filter(term);
      }
      public LogFrame GetMasterFrame()
      {
         return _database.GetMasterFrame();
      }
      public void AddFiles(string[] paths, bool processAsync = true, bool addFileAsync = true)
      {
         if (addFileAsync)
         {
            Task.Run(() =>
            {
               ProcessFiles(paths, processAsync);
            });

         }
         else
         {
            ProcessFiles(paths, processAsync);
         }

      }
      public LogFileInfo[] GetAllFileNames()
      {
         var lst = new List<LogFileInfo>();
         foreach (var at in _allFiles)
         {
            lst.Add(new LogFileInfo() { DisplayName = at.FileName, FileNo = at.UniqueFileNo, Key = "" });
         }
         return lst.ToArray();
      }
      public void ExportFiles(LogFileInfo[] fileList, string exportFilePath)
      {
         using (FileStream zipToOpen = new FileStream(
             exportFilePath,
             FileMode.OpenOrCreate))
         {
            var currentNo = 0;
            using (ZipArchive archive = new ZipArchive(
                zipToOpen,
                ZipArchiveMode.Update))
            {
               foreach (var fileKey in fileList)
               {
                  try
                  {
                     if (_allFiles.Any(x => x.UniqueFileNo == fileKey.FileNo))
                     {
                        //few errors here need to inform someone.
                        var fileObj = _allFiles.First(x => x.UniqueFileNo == fileKey.FileNo);
                        var zipFileItem = archive.CreateEntry(Path.GetFileName(fileObj.FileName));
                        var data = _ioHelper.GetFileData(fileObj);
                        using (var writer = new BinaryWriter(zipFileItem.Open()))
                        {
                           writer.Write(data);
                        }
                        currentNo++;
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
             "Copyright © 2021 Aryakal" +
             "Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files(the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/ or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:" +
             "The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software." +
             "THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE." +
             _pluginManger.GetAllPluginsInfo();
      }
      public FileParseProgressEvent GetStartInfo() 
      {       
         return _fileProgressStat;
      }

      private void ProcessFiles(string[] paths, bool useParallelTasks = true)
      {
         var lst = new List<FileObject>();
         long totalFileSize = 0;
         foreach (var fo in _ioHelper.expand_compresssed_files_and_files(paths))
         {
            fo.LogParser = _pluginManger.GuessParser(fo.FileName);
            if (null == fo.LogParser || _allFiles.Any(x => x.FilePath == fo.FilePath))
               continue;
            lst.Add(fo);
            fo.UniqueFileNo = ++UniqueFileId;
            totalFileSize += fo.FileSize;
         }

         if (0 == lst.Count)
            return;

         _fileProgressStat.TotalFileCount = lst.Count;

         var fileProgress = new FileParseProgressEvent() { TotalFileCount = lst.Count, ParseComplete = false };
         _messenger.PostMessage(fileProgress);
         _logger.LogInfo($"Total Files: {lst.Count} Size:  {Utils.GetHumanSize(totalFileSize)}");

         for (int ndx = 0; ndx < lst.Count; ndx++)
         {
            _logger.LogDebugProgress($"parsing [ {ndx} of {lst.Count}] ");
            var fileInfo = lst[ndx];
            var logFile = new LogFile(
                Path.GetFileName(fileInfo.FileName),
                _ioHelper.GetFileData(fileInfo)
                )
            {
               FileId = fileInfo.UniqueFileNo
            };
            _fileProgressStat.TotalFileSizeLoaded += logFile.FileData.Length;
            fileInfo.LogParser.ParseFile(logFile);

            _fileProgressStat.TotalFileLoadedCount++;
            _database.AddLogFile(logFile, logFile.FileId);
            _fileProgressStat.TotalFileParsedCount++;

            _messenger.PostMessage(fileProgress);

            logFile.Clear();
         }

         _allFiles.AddRange(lst);
         _database.OptimizeData();
         _messenger.PostMessage(LogAxeMessageEnum.EngineOptmizeComplete);
         Utils.ClearAllGCMemory();
         GC.Collect();
         _logger.LogDebug("optmizing data compelted");
         _messenger.PostMessage(LogAxeMessageEnum.NewViewAnnouncement);
         fileProgress.ParseComplete = true;
         _messenger.PostMessage(fileProgress);
      }
      //private CurrentResourceUsage GetCurrentUsage()
      //{
      //   return new CurrentResourceUsage()
      //   {
      //      //CurrentFileTotalSize = Utils.GetHumanSize(_totalFileSizeInSystem, true),
      //      TotalAppMemUsage = Utils.GetHumanSize(System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64, true),
      //      //TotalFiles = $"{_totalNoofFilesParsed.ToString()} of {_allFileInfo.Count.ToString()}" 
      //   };
      //}
   }
}
