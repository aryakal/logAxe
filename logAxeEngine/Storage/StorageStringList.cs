//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using logAxeEngine.Interfaces;

namespace logAxeEngine.Storage
{
   /// <summary>
   /// This is a simple string store or string interning,
   /// 
   /// Keeps only a single instance of the incomming data.
   /// Then make it to a list to access it faster.
   /// The idea here is when we are storing we need to know uniquness but not the access it faster.
   /// Once optimised we need to access it faster.
   /// 
   /// 
   /// Note
   ///     Not thread safe
   ///     If not optimized and we try to access data it will result in exception.
   /// </summary>
   public sealed class StorageStringList : IStorageString
   {
      private List<string> _uniqueStore = new List<string>(50000);
      public int Count => _uniqueStore.Count;


      public string RetriveString(int uniqueId)
      {
         if (uniqueId >= Count)
         {
            throw new Exception($"Index {uniqueId} is not in store of size {Count}");
         }
         return _uniqueStore[uniqueId];
      }

      public int StoreString(string data)
      {
         if (string.IsNullOrEmpty(data))
            return LogLine.INVALID;

         _uniqueStore.Add(data);
         return _uniqueStore.Count - 1;
      }

      public void OptimizeData()
      {

      }

      public void Clear()
      {
         _uniqueStore.Clear();
         _uniqueStore = new List<string>();
      }


      Dictionary<int, int> IStorageString.MergeStorage(IStorageString storage)
      {
         throw new System.NotImplementedException();
      }

      public int[] Filter(string[] includeMsgs, string[] excludeMsgs, bool caseSensitive = false, bool exactWord = false)
      {
         throw new System.NotImplementedException();
      }

   }
}
