//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace logAxeEngine.Interfaces
{

   /// <summary>
   /// Stores the strings for data.
   /// </summary>
   public interface IStorageString
   {
      /// <summary>
      /// Total strings currently in the system.
      /// </summary>
      int Count { get; }

      /// <summary>
      /// To keep the strings in the database.
      /// Call optmize at the end to use the database properly.
      /// </summary>
      /// <param name="data">data to be stored.</param>
      /// <returns>Uniuque id assigned.</returns>
      int StoreString(string data);

      /// <summary>
      /// Clear all the data in the system.
      /// </summary>
      void Clear();

      /// <summary>
      /// Get data or string for the unique id provided.
      /// </summary>
      /// <param name="uniqueId"></param>
      /// <returns></returns>
      string RetriveString(int uniqueId);

      /// <summary>
      /// Optimize data.
      /// </summary>
      void OptimizeData();

      /// <summary>
      /// This will merge the incomming database witht he current database and give the 
      /// translated values back.
      /// </summary>
      /// <param name="storage"></param>
      /// <returns></returns>
      Dictionary<int, int> MergeStorage(IStorageString storage);

      /// <summary>
      /// Searches the strings and gives all the ids that match.
      /// </summary>
      /// <param name="msgTerms">search strings not patters no regex support yet.</param>
      /// <returns>All ids which matched.</returns>
      int[] Filter(string[] includeMsgs,
          string[] excludeMsgs,
          bool caseSensitive = false,
          bool exactWord = false
          );
   }

   /// <summary>
   /// Stores the strings for data.
   /// </summary>
   public interface IStoreStringDB
   {
      /// <summary>
      /// Total strings currently in the system.
      /// </summary>
      int Count { get; }

      /// <summary>
      /// Clear all the data in the system.
      /// </summary>
      void Clear();

      /// <summary>
      /// Get data or string for the unique id provided.
      /// </summary>
      /// <param name="uniqueId"></param>
      /// <returns></returns>
      string RetriveString(int uniqueId);

      /// <summary>
      /// To keep the strings in the database.
      /// Call optmize at the end to use the database properly.
      /// </summary>
      /// <param name="data">data to be stored.</param>
      /// <returns>Uniuque id assigned.</returns>
      int StoreString(string data);

   }
}
