//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using logAxeEngine.Interfaces;

namespace logAxeEngine.Storage
{
   /// <summary>
   /// Stores the unique string. 
   ///   It creates a dictionary of StringRecord and integer. 
   ///   StringRecord contains Position in byte array and hash of the string. For details look into StringRecord struct.
   /// Optmization, when called will clear the dictionary in the current class.
   /// </summary>
   public class StorageStringDB : IStorageString
   {      
      public readonly int SearchStrategyThreshold = 30000 * 100;
      public int Count => _stringStore.Count;
      private StorageStringXEfficient _stringStore;
      private Dictionary<StringRecord, int> _stringMap;
      private bool IsOptmized = false;
      private int _searchStategy = 0;

      public StorageStringDB(
         int pagePerbook = 10, 
         int dataPerPage = 1024 * 1024 * 50, 
         int possibleStringSize = 1024 * 1024 * 1)
      {         
         _stringStore = new StorageStringXEfficient(pagePerbook, dataPerPage, possibleStringSize);

         //This is a special constructor of dicusiton where StorageStringXEfficient is used as comparer.
         _stringMap = new Dictionary<StringRecord, int>(_stringStore);         
      }

      public void Clear()
      {  
         _stringStore.Clear();
         _stringMap.Clear();
      }
      public Dictionary<int, int> MergeStorage(IStorageString storage)
      {
         var maps = new Dictionary<int, int>();

         if (typeof(StorageStringList) == storage.GetType())
         {
            for (int ndx = 0; ndx < storage.Count; ndx++)
            {
               var data = storage.RetriveString(ndx);
               int id = StoreString(data);
               maps.Add(ndx, id);
            }
            return maps;
         }
         else
         {
            throw new NotImplementedException();
         }
      }
      public void OptimizeData()
      {
         _stringMap.Clear();
         _stringMap = null;
         //This is a special constructor of dicusiton where StorageStringXEfficient is used as comparer.
         _stringMap = new Dictionary<StringRecord, int>(_stringStore);
         IsOptmized = true;
      }
      public string RetriveString(int uniqueId)
      {
         return _stringStore.RetriveString(uniqueId);
      }
      public int StoreString(string data)
      {
         if (IsOptmized)
         {
            for (int ndx = 0; ndx < _stringStore.Count; ndx++)
            {
               var st = new StringRecord { InternalStringID = ndx, StringHash = _stringStore.RetriveString(ndx).GetHashCode() };
               _stringMap.Add(st, ndx);
            }
            IsOptmized = false;
         }

         var stData = new StringRecord { InternalStringID = -1, StringData = data, StringHash = data.GetHashCode() };
         if (!_stringMap.ContainsKey(stData))
         {
            var uniqueId = _stringStore.StoreString(data);
            stData.InternalStringID = uniqueId;
            stData.StringData = null;
            _stringMap.Add(stData, uniqueId);            
            return uniqueId;
         }
         return _stringMap[stData];
      }
      public int[] Filter(string[] includeMsgs, string[] excludeMsgs, bool caseSensitive = false, bool exactWord = false)
      {
         //TODO : search for all scenario where excluded msg is already in include message.
         var allMsg = new bool[Count];
         var ordinalType = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

         // Scenario when the we want to filter out certain message.
         // So we need to make all the messages are included.
         if (includeMsgs.Length == 0 && excludeMsgs.Length != 0)
         {
            for (var ndx = 0; ndx < Count; ndx++)
            {
               allMsg[ndx] = true;
            }
         }

         if (!exactWord)
         {
            _searchStategy = Count >= SearchStrategyThreshold ? 1 : 0;

            if (0 == _searchStategy)
            {
               for (var ndx = 0; ndx < Count; ndx++)
               {
                  var msg = RetriveString(ndx);
                  foreach (var term in includeMsgs)
                  {
                     if (msg.IndexOf(term, ordinalType) == -1) continue;
                     allMsg[ndx] = true;
                     break;
                  }

                  foreach (var term in excludeMsgs)
                  {
                     if (msg.IndexOf(term, ordinalType) == -1) continue;
                     allMsg[ndx] = false;
                     break;
                  }
               }
            }
            else if (1 == _searchStategy)
            {
               foreach (var term in includeMsgs)
               {
                  Parallel.For(0, Count, ndx =>
                  {
                     var msg = RetriveString(ndx);
                     if (!allMsg[ndx] && msg.IndexOf(term, ordinalType) == -1) return;
                     allMsg[ndx] = true;
                  });
               }

               foreach (var term in excludeMsgs)
               {
                  Parallel.For(0, Count, ndx =>
                  {
                     var msg = RetriveString(ndx);
                     if (allMsg[ndx] && msg.IndexOf(term, ordinalType) == -1) return;
                     allMsg[ndx] = false;
                  });
               }
            }

         }
         else
         {
            Parallel.For(0, Count, ndx =>
            {
               var msg = RetriveString(ndx);
               foreach (var term in includeMsgs)
               {
                  if (msg != term) continue;
                  allMsg[ndx] = true;
                  break;

               }

               foreach (var term in excludeMsgs)
               {
                  if (msg != term) continue;
                  allMsg[ndx] = false;
                  break;
               }
            });
         }



         var result = new List<int>();
         for (var ndx = 0; ndx < Count; ndx++)
         {
            if (allMsg[ndx]) result.Add(ndx);
         }

         return result.ToArray();
      }

   }
}
