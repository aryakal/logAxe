using logAxeCommon;
using logAxeEngine.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using logAxeEngine.Interfaces;

namespace logAxeEngine.Storage
{
   //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types

   public sealed class StorageMetaDatabase : IStorageDataBase
   {
      private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

      private struct InterLogLine
      {
         public long TimeStamp;
         public int MsgId;
         public int StackId;
         public int TagId;
         public int FileIndex;
         public int LineNumber;
         public byte LogType;
      }

      private struct TempInterLogLine
      {
         public long TimeStamp;
         public int InternalGlobalLine;
      }

      private GenericHugeStore<InterLogLine> _store;

      int[] _globalLogLines;
      private bool _isOptmized;

      private int[][] _indexMessages;
      private int[][] _indexTags;
      private int[][] _indexLogType;

      private IStorageString _dbString;
      private IStorageString _dbTag;
      private const int BookIndexSize = 10;

      LogFrame _mainFrame;
      public int TotalLogLines => _store.Count;

      private NamedLogger _logger = new NamedLogger("metaDb");

      public StorageMetaDatabase(
          IStorageString stringStore,
          IStorageString tagStore,
          int pageSize = 100)
      {
         _store = new GenericHugeStore<InterLogLine>(10, 30000);
         _dbString = stringStore;
         _dbTag = tagStore;
         _mainFrame = new LogFrame(
             0,
             0,
             null,
             null,
             null);

      }

      public void AddLogFile(LogFile logFile, int fileNumber)
      {
         _isOptmized = false;
         //Store in internal global line !
         var mergedStore = _dbString.MergeStorage(logFile.StringStore);
         var mergedTags = _dbTag.MergeStorage(logFile.DBTags);
         for (int ndx = 0; ndx < logFile.TotalLines; ndx++)
         {
            var logLine = logFile.LogLines[ndx];
            var st = new InterLogLine()
            {
               FileIndex = logLine.FileNumber == LogLine.INVALID ? fileNumber : logLine.FileNumber,
               LineNumber = logLine.LineNumber,
               LogType = (byte)logLine.LogType,
               MsgId = logLine.MsgId >= 0 ? mergedStore[logLine.MsgId] : LogLine.INVALID,
               StackId = logLine.StackTraceId != LogLine.INVALID ? mergedStore[logLine.StackTraceId] : LogLine.INVALID,
               TagId = logLine.TagId >= 0 ? mergedTags[logLine.TagId] : LogLine.INVALID,
               TimeStamp = logLine.TimeStamp.ToFileTimeUtc(), // review
            };
            _store.StoreData(st);
         }
      }
      public void Clear()
      {
         _mainFrame = new LogFrame(
             0,
             0,
             null,
             null,
             null);
         _store.Clear();
         _dbString.Clear();
         _dbTag.Clear();
         _isOptmized = false;
      }
      public LogLine GetLogLine(int globalLineNumber)
      {
         var st = GetTranslatedLine(globalLineNumber);

         return new LogLine()
         {
            FileNumber = st.FileIndex,
            LineNumber = st.LineNumber,
            LogType = (LogType)st.LogType,
            MsgId = st.MsgId,
            StackTraceId = st.StackId,
            TagId = st.TagId,
            TimeStamp = DateTime.FromFileTimeUtc(st.TimeStamp)
         };
      }
      public LogType GetLogLineType(int globalLineNumber)
      {
         return (LogType)GetTranslatedLine(globalLineNumber).LogType;
      }
      public LogFrame GetMasterFrame()
      {
         return _mainFrame.GetCloneCopy();
      }
      public void OptimizeData()
      {
         if (_isOptmized) return;
         var totalLogLines = _store.Count;
         _dbString.OptimizeData();
         _dbTag.OptimizeData();
         _logger.LogDebug($"Indexing Lines: {_store.Count}, Strings: {_dbString.Count}, Tags: {_dbTag.Count}");

         _globalLogLines = new int[totalLogLines];

         var tempSort = new TempInterLogLine[totalLogLines];
         Parallel.For(0, totalLogLines, ndx =>
         {
            var st = _store.RetriveData(ndx);
            tempSort[ndx].TimeStamp = st.TimeStamp;
            tempSort[ndx].InternalGlobalLine = ndx;
         });
         //The main sort of the whole application.
         tempSort = tempSort.OrderBy(stamp => stamp.TimeStamp).ToArray();

         Parallel.For(0, totalLogLines, ndx =>
         {
            _globalLogLines[ndx] = tempSort[ndx].InternalGlobalLine;
         });
         tempSort = null;
         CreateSortedIndexes();
         _isOptmized = true;
      }

      private struct TempIndexId
      {
         public int MsgId;
         public int TagId;
         public int LogType;
         public int InternalGlobalLine;
      }

      private void CreateSortedIndexes3()
      {
         var totalLogLines = _store.Count;
         var tempMaster = new TempIndexId[totalLogLines];
         //we can reference the original line not traslated if requred.
         Parallel.For(0, totalLogLines, ndx =>
         {
            var st = GetTranslatedLine(ndx);
            tempMaster[ndx].MsgId = st.MsgId;
            tempMaster[ndx].TagId = st.TagId;
            tempMaster[ndx].LogType = st.LogType;
            tempMaster[ndx].InternalGlobalLine = ndx;
         });

         var sortedMessage = tempMaster.OrderBy(stamp => stamp.MsgId).ToArray();
         var sortedTag = tempMaster.OrderBy(stamp => stamp.TagId).ToArray();
         var sortedLogType = tempMaster.OrderBy(stamp => stamp.LogType).ToArray();


         //how to store
         //var currentId = 0;
         //var length = 0;
         //var _messages = new int[totalLogLines];
         //var _messagesLength = new int[totalLogLines];
         //for (var ndx = 0; ndx < tempMaster.Length; ndx++)
         //{
         //    var st = tempMaster[ndx];
         //    _logger.LogDebug($"line {st.InternalGlobalLine}, {st.MsgId}");
         //}
      }

      private void CreateSortedIndexes2()
      {
         var totalLogLines = _store.Count;
         var sortedMessages = new int[_dbString.Count + 10];
         var sortedTags = new int[_dbTag.Count + 10];
         var sortedLogType = new int[4];

         Parallel.For(0, totalLogLines, ndx =>
         {
            var st = GetTranslatedLine(ndx);
            Interlocked.Increment(ref sortedMessages[st.MsgId]);
            Interlocked.Increment(ref sortedTags[st.TagId]);
            Interlocked.Increment(ref sortedLogType[st.LogType]);
         });

         _indexMessages = CreateEmptyIntArray(sortedMessages);
         _indexLogType = CreateEmptyIntArray(sortedLogType);
         _indexTags = CreateEmptyIntArray(sortedTags);

         _indexLogType = new int[sortedMessages.Length][];
         Parallel.For(0, totalLogLines, ndx =>
         {
            var st = GetTranslatedLine(ndx);
            Interlocked.Increment(ref sortedMessages[st.MsgId]);
            Interlocked.Increment(ref sortedTags[st.TagId]);
            Interlocked.Increment(ref sortedLogType[st.LogType]);
         });
      }
      private int[][] CreateEmptyIntArray(int[] input)
      {
         var ret = new int[input.Length][];
         for (var ndx = 0; ndx < input.Length; ndx++)
         {
            ret[ndx] = new int[input[ndx]];
         }
         return ret;
      }
      private void CreateSortedIndexes()
      {
         //TODO : String id is  not matching with index of storage.


         /*
          *  2 | 3 | 2 |
          * 2 - 0, 2
          * 3 - 1
          */


         var totalLogLines = _store.Count;
         var sortedMessages = HelperCreateEmptyIndex(_dbString.Count + 10);
         var sortedTags = HelperCreateEmptyIndex(_dbTag.Count + 10);
         var sortedLogType = HelperCreateEmptyIndex(4);
         _logger.LogDebug($"Created the lists");
         for (var ndx = 0; ndx < totalLogLines; ndx++)
         {
            var st = GetTranslatedLine(ndx);
            try
            {
               sortedMessages[st.MsgId].Add(ndx);
               if (st.TagId != LogLine.INVALID)
                  sortedTags[st.TagId].Add(ndx);
               sortedLogType[st.LogType].Add(ndx);
            }
            catch
            {
               _logger.LogError($"Index {ndx}, {st.MsgId}, {st.TagId}, {st.LogType}");
               throw;
            }
         }

         _logger.LogDebug($"stored in the lists");

         //_indexMessages = HelperCreateIndex(sortedMessages);
         //_indexLogType = HelperCreateIndex(sortedLogType);
         //_indexTags = HelperCreateIndex(sortedTags);

         var tskMsg = Task.Run(() =>
         {
            _indexMessages = HelperCreateIndex(sortedMessages);
         });

         var tskTag = Task.Run(() =>
         {
            _indexTags = HelperCreateIndex(sortedTags);
         });

         var tskLog = Task.Run(() =>
         {
            _indexLogType = HelperCreateIndex(sortedLogType);
         });

         tskMsg.Wait();
         tskTag.Wait();
         tskLog.Wait();

         _logger.LogDebug($"completed storing in indexes");




         //Lets create the frame here
         var logLineTypes = new LogType[totalLogLines];
         var logLineLength = new int[4];
         Parallel.For(0, totalLogLines, ndx =>
         {
            var type = GetLogLineType(ndx);
            logLineTypes[ndx] = type;
            Interlocked.Increment(ref logLineLength[(byte)type]);
         });

         _mainFrame = new LogFrame(
            totalLogLines,
            totalLogLines,
            logLineTypes,
            logLineLength,
            null);

      }
      public LogFrame Filter(TermFilter term)
      {

         if (term == null) return _mainFrame.GetCloneCopy();
         if (TotalLogLines == 0) return _mainFrame.GetCloneCopy();

         var TotalLogTypeFilters = term.TotalLogTypeFilters;
         if (TotalLogTypeFilters == 0) return GetMasterFrame();

         var stringResult = new int[0];
         var tagResult = new int[0];

         var logTypes = new List<int>();
         var allMessages = new List<int>();
         var allTags = new List<int>();

         var allIncludeMsgs = term.MsgInclude.ToList();
         //allIncludeMsgs.AddRange(term.TagsInclude);

         var allExcludeMsgs = term.MsgExclude.ToList();
         //allExcludeMsgs.AddRange(term.TagsExclude);
         var result = new int[0];

         var stringTsk = Task.Run(() =>
         {
            stringResult = _dbString.Filter(
                   allIncludeMsgs.ToArray(),
                   allExcludeMsgs.ToArray(),
                   term.MatchCase,
                   term.MatchExact);
            for (var ndx = 0; ndx < stringResult.Length; ndx++)
            {
               allMessages.AddRange(_indexMessages[stringResult[ndx]]);
            }
         });
         bool tagFilterUsed = (term.TagsInclude.Length + term.TagsExclude.Length) != 0;
         Task tagTsk = Task.Run(() =>
         {
            if (tagFilterUsed)
            {
               tagResult = _dbTag.Filter(
                  term.TagsInclude.ToArray(),
                  term.TagsExclude.ToArray(),
                  term.MatchCase,
                  term.MatchExact);
               for (var ndx = 0; ndx < tagResult.Length; ndx++)
               {
                  allTags.AddRange(_indexTags[tagResult[ndx]]);
               }
            }

         });

         if (TotalLogTypeFilters != 4)
         {
            for (var ndx = 0; ndx < 4; ndx++)
            {
               if (term.FilterTraces[ndx])
                  logTypes.AddRange(_indexLogType[ndx]);
            }
            result = logTypes.ToArray();
         }
         else
         {
         }

         tagTsk.Wait();
         stringTsk.Wait();

         if (allTags.Count != 0)
         {
            allMessages = allMessages.Count == 0 ? allTags : allTags.Union(allMessages).ToList();
         }

         // Filter code

         if (allMessages.Count != 0)
         {
            result = result.Length != 0 ? allMessages.Intersect(result).ToArray() : allMessages.ToArray();
         }
         Array.Sort(result);

         if (result.Length != 0)

         {
            var logLineTypes = new LogType[result.Length];
            var logLineLength = new int[4];
            Parallel.For(0, result.Length, ndx =>
            {
               var type = GetLogLineType(result[ndx]);
               logLineTypes[ndx] = type;
               Interlocked.Increment(ref logLineLength[(int)type]);
            });

            return new LogFrame(
                _store.Count,
                result.Length,
                logLineTypes,
                logLineLength,
                result);
         }
         else
         {
            return new LogFrame(
                _store.Count,
                result.Length,
                new LogType[0],
                new int[] { 0, 0, 0, 0 },
                result);
         }


      }

      private int[][] HelperCreateIndex(List<int>[] lst)
      {
         var sortedIndex = new int[lst.Length + 1][];
         for (var ndx = 0; ndx < lst.Length; ndx++)
         {
            //Todo search for null if message is hanging there.
            sortedIndex[ndx] = lst[ndx]?.ToArray();
            lst[ndx].Clear();
            lst[ndx] = null;
         }
         //Parallel.For(0, lst.Length, ndx =>
         //{
         //    sortedIndex[ndx] = lst[ndx].ToArray();
         //    lst[ndx].Clear();
         //    lst[ndx] = null;
         //});
         return sortedIndex;
      }
      private List<int>[] HelperCreateEmptyIndex(int index)
      {
         var newlst = new List<int>[index];
         for (var ndx = 0; ndx < newlst.Length; ndx++)
         {
            newlst[ndx] = new List<int>();
         }
         return newlst;

      }
      private InterLogLine GetTranslatedLine(int index)
      {
         return _store.RetriveData(_globalLogLines[index]);
      }
   }
}
