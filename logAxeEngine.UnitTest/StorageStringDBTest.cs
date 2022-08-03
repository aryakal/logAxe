//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using logAxeEngine.Interfaces;
using logAxeEngine.Storage;

namespace logAxeEngine.UnitTest
{
   [TestClass]
   public class StorageStringDBTest
   {
      IStorageString _sut;

      [TestInitialize]
      public void Setup()
      {
         _sut = new StorageStringDB();
      }

      /// <summary>
      /// Test sanity of the store string.
      /// Does the following test
      /// > adds 10 string two times.
      /// > optmized data
      /// > check if the data is retained, makes sure that after the data is optmized the information is retained.
      /// > optmized data
      /// > add same data (check)
      /// > clears all string
      /// > checks total count.
      /// </summary>
      [TestMethod]
      public void TestSanity()      
      {
         //Checks for duplicate string
         for (int ndx = 0; ndx < 10; ndx++)
         {
            var duplicateString = $"duplicateString{ndx}";
            int id1 = _sut.StoreString(duplicateString);
            int id2 = _sut.StoreString(duplicateString);
            Assert.IsTrue(id1 == ndx);
            Assert.IsTrue(id1 == id2);
            Assert.IsTrue(duplicateString == _sut.RetriveString(id1));
         }

         _sut.OptimizeData();
         for (int ndx = 0; ndx < 10; ndx++)
         {
            var duplicateString = $"duplicateString{ndx}";
            Assert.IsTrue(duplicateString == _sut.RetriveString(ndx));
         }

         _sut.OptimizeData();
         for (int ndx = 0; ndx < 10; ndx++)
         {
            var duplicateString = $"duplicateString{ndx}";
            int id1 = _sut.StoreString(duplicateString);
            Assert.IsTrue(ndx == id1);
            Assert.IsTrue(duplicateString == _sut.RetriveString(ndx));
         }
         _sut.Clear();
         Assert.IsTrue(_sut.Count == 0);
      }

      [TestMethod]
      public void TestMergeStorage()
      {
         Assert.ThrowsException<NotImplementedException>(() => _sut.MergeStorage(new StorageStringDB()));
      }
   }
}
