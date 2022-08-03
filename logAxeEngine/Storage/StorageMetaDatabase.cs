//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using logAxeCommon;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;
using libALogger;

namespace logAxeEngine.Storage
{
   //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types

   public sealed class StorageMetaDatabase : IStorageDataBase
   {
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

      private ILibALogger _logger;
      private GenericHugeStore<InterLogLine> _store;      
      private bool _isOptmized;
      private int[] _globalLogLines;
      private int[][] _indexMessages;
      private int[][] _indexTags;
      private int[][] _indexLogType;

      private IStorageString _dbString;
      private IStorageString _dbTag;      
      private LogFrame _mainFrame;
      
      public int TotalLogLines => _store.Count;
      public StorageMetaDatabase(         
          IStorageString stringStore,
          IStorageString tagStore,
          int pageSize = 100)
      {
         _logger = Logging.GetLogger("metaDb");
         _store = new GenericHugeStore<InterLogLine>(10, 30000);
         _dbString = stringStore;
         _dbTag = tagStore;
         _mainFrame = LogFrame.GetEmptyView();
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
         _mainFrame = LogFrame.GetEmptyView();
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

         _dbString.OptimizeData();
         _dbTag.OptimizeData();

         var totalLogLines = _store.Count;
         _logger.Debug($"Indexing Lines: {_store.Count}, Strings: {_dbString.Count}, Tags: {_dbTag.Count}");
         _globalLogLines = new int[totalLogLines];
         var tempSort = new TempInterLogLine[totalLogLines];
         Parallel.For(0, totalLogLines, ndx =>
         {
            var st = _store.RetriveData(ndx);
            tempSort[ndx].TimeStamp = st.TimeStamp;
            tempSort[ndx].InternalGlobalLine = ndx;
         });

         //TODO : - improve time to optimze time.         
         //tempSort = tempSort.OrderBy(stamp => stamp.TimeStamp).ToArray(); // bad idea with huge array it will crash.q
         Array.Sort(tempSort, delegate (TempInterLogLine x, TempInterLogLine y) { return x.TimeStamp.CompareTo(y.TimeStamp); });

         Parallel.For(0, totalLogLines, ndx =>
         {
            _globalLogLines[ndx] = tempSort[ndx].InternalGlobalLine;
         });
         tempSort = null;

         CreateSortedIndexes();

         _isOptmized = true;
      }
      public LogFrame Filter(TermFilter term)
      {

         if (term == null) return GetMasterFrame();
         if (TotalLogLines == 0) return GetMasterFrame();
         if (term.TotalLogTypeFilters == 0) return LogFrame.GetEmptyView();
         if (term.IsMasterFilter) return GetMasterFrame();

         var TotalLogTypeFilters = term.TotalLogTypeFilters;
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
         _logger.Debug($"Created the lists");
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
               _logger.Error($"Index {ndx}, {st.MsgId}, {st.TagId}, {st.LogType}");
               throw;
            }
         }

         _logger.Debug($"stored in the lists");

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

         _logger.Debug($"completed storing in indexes");

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
