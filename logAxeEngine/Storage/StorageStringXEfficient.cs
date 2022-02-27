//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Text;
using logAxeEngine.Common;
using logAxeEngine.Interfaces;

namespace logAxeEngine.Storage
{
   public sealed class StorageStringXEfficient : IStorageString, IEqualityComparer<StringGhost>
   {
      private byte[][] _hugeStore;
      private List<long> _stringPositions;
      private readonly int _pagePerBook;
      private readonly int _dataPerPage;
      private readonly int _possibleStringSize;

      private Int64 _currentWriteIndex = 0;

      private int _currentId = -1;

      public int Count => _currentId + 1;

      private NamedLogger _logger = new NamedLogger("strStore");

      public StorageStringXEfficient(int pagePerbook = 10, int dataPerPage = 1024 * 1024 * 50, int possibleStringSize = 1024 * 1024 * 1)
      {
         _pagePerBook = pagePerbook;
         _dataPerPage = dataPerPage;
         _possibleStringSize = possibleStringSize;
         Clear();//lets reset it.
      }

      #region IStorageString
      public void Clear()
      {
         if (_hugeStore != null)
         {
            //HelperDumpData();
            for (var ndx = 0; ndx < _hugeStore.GetLength(0); ndx++)
            {
               _hugeStore[ndx] = null;
            }

            _hugeStore = null;
            _stringPositions = null;
         }
         _hugeStore = new byte[_pagePerBook][];
         _hugeStore[0] = new byte[_dataPerPage];
         _stringPositions = new List<long>(_possibleStringSize);
         _currentWriteIndex = 0;
         _currentId = -1;
         _stringPositions.Add(0);
      }
      public string RetriveString(int uniqueId)
      {
         //--uniqueId;
         var (startPage, startLocation, endPage, endLocation) = GetLocationIndexs(
             _stringPositions[uniqueId], //The first index
             _stringPositions[uniqueId + 1] - _stringPositions[uniqueId]); // The length of the string.

         if (startPage != endPage)
         {
            return
                Encoding.UTF8.GetString(_hugeStore[startPage], startLocation, _hugeStore[startPage].Length - startLocation) +
                Encoding.UTF8.GetString(_hugeStore[endPage], 0, endLocation);
         }
         else
         {
            return Encoding.UTF8.GetString(_hugeStore[startPage], startLocation, (endLocation - startLocation));
         }

      }
      public int StoreString(string data)
      {
         var byteData = Encoding.UTF8.GetBytes(data);

         var (startPage, startLocation, endPage, endLocation) = GetLocationIndexs(_currentWriteIndex, byteData.Length);

         if (startPage != endPage)
         {
            AddPage(endPage);
            var length = _hugeStore[startPage].Length - startLocation;
            Buffer.BlockCopy(byteData, 0, _hugeStore[startPage], startLocation, length);
            Buffer.BlockCopy(byteData, length, _hugeStore[endPage], 0, byteData.Length - length);
         }
         else
         {
            Buffer.BlockCopy(byteData, 0, _hugeStore[startPage], startLocation, byteData.Length);
         }

         _currentWriteIndex += byteData.Length;
         //_stringPositions[++Count] = _currentWriteIndex;
         _stringPositions.Add(_currentWriteIndex);
         _currentId++;
         return _currentId;
      }
      public void OptimizeData()
      {
      }
      public Dictionary<int, int> MergeStorage(IStorageString storage)
      {
         throw new NotImplementedException();
      }
      public int[] Filter(string[] includeMsgs, string[] excludeMsgs, bool caseSensitive = false, bool exactWord = false)
      {
         throw new NotImplementedException();
      }
      #endregion

      #region IEqualityComparer<StringGhost>
      public bool Equals(StringGhost x, StringGhost y)
      {
         return RetriveString(x.Position).Equals(y.Data);
      }
      public int GetHashCode(StringGhost obj)
      {
         return obj.Hash;
      }
      #endregion

      public void AddPage(int endPage)
      {
         if (endPage >= _hugeStore.GetLength(0))
         {
            var temp = new byte[_hugeStore.GetLength(0) + _pagePerBook][];
            for (var ndx = 0; ndx < _hugeStore.GetLength(0); ndx++)
            {
               temp[ndx] = _hugeStore[ndx];
               _hugeStore[ndx] = null;
            }
            _hugeStore = null;
            _hugeStore = temp;
            temp = null;
         }
         _hugeStore[endPage] = new byte[_dataPerPage];
      }

      private Tuple<int, int> GetLocation(long index)
      {
         return Tuple.Create(
             Convert.ToInt32(index / _dataPerPage),
             Convert.ToInt32(index % _dataPerPage));
      }

      private Tuple<int, int, int, int> GetLocationIndexs(long index, long dataLength)
      {
         var (startPage, startLocation) = GetLocation(index);
         var (endPage, endLocation) = GetLocation(index + dataLength);
         return Tuple.Create(
             startPage,
             startLocation,
             endPage,
             endLocation
             );
      }
      //private void HelperDumpData()
      //{
      //    long len = 0;
      //    int totalIndex = 0;
      //    for (var ndx = 0; ndx < _hugeStore.GetLength(0); ndx++)
      //    {

      //        if (_hugeStore[ndx] != null)
      //        {
      //            len += _hugeStore[ndx].Length;
      //            totalIndex++;
      //        }
      //    }
      //    _logger.LogDebug($"Removing String {Count} Stored in Indexs: {totalIndex} Capacity: {Utils.GetHumanSize(len)} Filled: {Utils.GetHumanSize(_currentWriteIndex)}");
      //}
   }

   public struct StringGhost
   {
      public int Position;
      public int Hash;
      public string Data;
   }
}
