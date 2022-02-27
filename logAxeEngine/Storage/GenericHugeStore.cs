//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using System;

namespace logAxeEngine.Storage
{
   public sealed class GenericHugeStore<T>
   {
      private T[][] _book;
      private readonly int _pagePerBook;
      private readonly int _linesPerPage;
      private int _nexIndexToCheck = 0;
      private int _currentPage = 0;
      private int _currentLine = 0;
      public int Count { get; private set; } = 0;

      public GenericHugeStore(int noOfPages = 10, int noOfLines = 10000)
      {
         _pagePerBook = noOfPages;
         _linesPerPage = noOfLines;
         Clear();//lets reset it.
      }

      public int StoreData(T data)
      {
         if (Count >= _nexIndexToCheck)
         {
            _currentPage++;
            _nexIndexToCheck += _linesPerPage;

            if (_currentPage >= _book.GetLength(0))
            {
               var temp = new T[_book.GetLength(0) + _pagePerBook][];
               for (var ndx = 0; ndx < _book.GetLength(0); ndx++)
               {
                  temp[ndx] = _book[ndx];
                  _book[ndx] = null;
               }
               _book = null;
               _book = temp;
            }
            _book[_currentPage] = new T[_linesPerPage];
            _currentLine = 0;
         }

         _book[_currentPage][_currentLine] = data;
         _currentLine++;
         Count++;
         return Count - 1;
      }

      public T RetriveData(int index)
      {
         if (index >= Count)
         {
            throw new Exception("out of index exception");
         }

         var pageNum = index / _linesPerPage;
         var lineNum = index % _linesPerPage;
         return _book[pageNum][lineNum];
      }

      public void Clear()
      {
         if (_book != null)
         {
            for (var ndx = 0; ndx < _book.GetLength(0); ndx++)
            {
               if (_book[ndx] != null)
               {
                  _book[ndx] = null;
               }
            }
         }

         Count = 0;
         _book = null;
         _currentPage = 0;
         _currentLine = 0;
         _book = new T[_pagePerBook][];
         _book[0] = new T[_linesPerPage];
         _nexIndexToCheck = _linesPerPage;
      }

      private void GetLocation(int index, out int bookNum, out int pageNum)
      {
         bookNum = index / _linesPerPage;
         pageNum = index % _linesPerPage;
      }
   }
}
