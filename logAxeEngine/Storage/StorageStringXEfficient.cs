//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;
using System.Collections.Generic;
using System.Text;
using logAxeEngine.Interfaces;
using logAxeCommon;
using libALogger;

namespace logAxeEngine.Storage
{
   public class StorageStringXEfficient : IStorageString, IEqualityComparer<StringRecord>
   {
      private byte[][] _hugeStore;
      private List<long> _stringPositions;
      private readonly int _pagePerBook;
      private readonly int _dataPerPage;
      private readonly int _possibleStringSize;

      private Int64 _currentWriteIndex = 0;
      private int _currentId = 0;
      public int Count => _currentId;

      private ILibALogger _logger;
      /// <summary>
      /// StorageStringXEfficient class stores the string more efficently.
      /// It stores as a byte array in internal byte array which is arrange as a page.
      /// </summary>
      /// <param name="pagePerbook">No of pages, or byte array(s)</param>
      /// <param name="dataPerPage">Byte array size</param>
      /// <param name="possibleStringSize">Size of each string.</param>
      public StorageStringXEfficient(int pagePerbook = 10, int dataPerPage = 1024 * 1024 * 50, int possibleStringSize = 1024 * 1024 * 1)
      {
         _logger = Logging.GetLogger("dbStr");
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
         _currentId = 0;
         _stringPositions.Add(0);
      }
      public string RetriveString(int uniqueId)
      {
         var (startPage, startLocation, endPage, endLocation) = GetLocationIndexs(
             _stringPositions[uniqueId], //The first index
             _stringPositions[uniqueId + 1] - _stringPositions[uniqueId]); // The length of the string.

         if (startPage != endPage)
         {
            var sb = new StringBuilder();            
            for (var ndx = startPage; ndx <= endPage; ndx++)
            {
               if (ndx == startPage)
               {
                  sb.Append(Encoding.UTF8.GetString(_hugeStore[ndx], startLocation, _dataPerPage-startLocation));
               }
               else if (ndx == endPage)
               {
                  sb.Append(Encoding.UTF8.GetString(_hugeStore[ndx], 0, endLocation));
               }
               else
               {  
                  sb.Append(Encoding.UTF8.GetString(_hugeStore[ndx], 0, _dataPerPage));
               }
            }
            return sb.ToString();            
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
            var startPos = 0;
            for (var ndx = startPage; ndx <= endPage; ndx++)
            {
               InitPage(ndx);               
               if (ndx == startPage)
               {
                  Buffer.BlockCopy(byteData, startPos, _hugeStore[ndx], startLocation, _dataPerPage - startLocation);
                  startPos = _dataPerPage - startLocation;
               }
               else if (ndx == endPage)
               {
                  Buffer.BlockCopy(byteData, startPos, _hugeStore[ndx], 0, endLocation);
               }
               else 
               {
                  Buffer.BlockCopy(byteData, startPos, _hugeStore[ndx], 0, _dataPerPage);
                  startPos += _dataPerPage;
               }
            }
         }
         else
         {
            InitPage(startPage);
            Buffer.BlockCopy(byteData, 0, _hugeStore[startPage], startLocation, byteData.Length);
         }
         _currentWriteIndex += byteData.Length;
         _stringPositions.Add(_currentWriteIndex);
         _currentId++;
         return _currentId - 1;
      }
      public void OptimizeData()
      {
         throw new NotImplementedException();
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
      public bool Equals(StringRecord x, StringRecord y)
      {
         return RetriveString(x.InternalStringID).Equals(y.StringData);
      }
      public int GetHashCode(StringRecord x)
      {
         return x.StringHash;
      }
      #endregion

      private void InitPage(int pageNo)
      {
         if (pageNo >= _hugeStore.GetLength(0))
         {
            AddMorePages();
         }

         if (null == _hugeStore[pageNo])
            _hugeStore[pageNo] = new byte[_dataPerPage];
      }

      private void AddMorePages()
      {
         //Strategy : increase the page by 2 times or add equal no of pages.
         //var temp = new byte[_hugeStore.GetLength(0) * 2][];

         var temp = new byte[_hugeStore.GetLength(0) + _pagePerBook][];
         for (var ndx = 0; ndx < _hugeStore.GetLength(0); ndx++)
         {
            temp[ndx] = _hugeStore[ndx];
            _hugeStore[ndx] = null;
         }
         _hugeStore = null;
         _hugeStore = temp;
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
   }

   /// <summary>
   /// Struct used temporarilty to use the Dictionary infrastructure to find the duplicates and used in 
   /// IEqualityComparer of StorageStringXEfficient
   /// </summary>
   public struct StringRecord
   {
      /// <summary>
      /// This is the id generated by StorageStringXEfficient
      /// </summary>
      public int InternalStringID;
      /// <summary>
      /// The has of the string data.
      /// </summary>
      public int StringHash;
      /// <summary>
      /// The data kept temporarily in memory.
      /// </summary>
      public string StringData;
   }
}
