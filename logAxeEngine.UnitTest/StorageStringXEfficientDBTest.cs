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
   public class StorageStringXEfficientDBTest
   {      

      [TestMethod]
      public void TestInterface()
      {
         var sut = new StorageStringXEfficient();         
         Assert.ThrowsException<NotImplementedException>(() => sut.MergeStorage(null));
         Assert.ThrowsException<NotImplementedException>(() => sut.Filter(null, null));
         Assert.ThrowsException<NotImplementedException>(() => sut.OptimizeData());
         
      }

      [TestMethod]
      public void TestBoundarySanity()
      {
         var sut = new StorageStringXEfficient(pagePerbook: 2, dataPerPage: 4, possibleStringSize: 5);

         Assert.IsTrue(0 == sut.StoreString("0123456789"));
         Assert.IsTrue(1 == sut.StoreString("a"));
         Assert.IsTrue(2 == sut.StoreString("abc"));

         Assert.IsTrue("0123456789" == sut.RetriveString(0));
         Assert.IsTrue("a" == sut.RetriveString(1));
         Assert.IsTrue("abc" == sut.RetriveString(2));

      }

      [TestMethod]
      public void TestSanity()
      {
         var sut = new StorageStringXEfficient(pagePerbook: 10, dataPerPage: 9, possibleStringSize: 5);         
         for (int ndx = 0; ndx < 1000; ndx++)
         {
            Assert.IsTrue(ndx == sut.StoreString("0123456789"));
         }

         for (int ndx = 0; ndx < 1000; ndx++)
         {
            var data = sut.RetriveString(ndx);
            Assert.IsTrue("0123456789" == data);
         }

      }

      //[TestMethod]
      //public void TestStorage()
      //{
      //   var sut = new StorageStringXEfficient(pagePerbook: 2, dataPerPage: 10, possibleStringSize: 5);
      //   var stringId1 = sut.StoreString("1234567890");
      //   var stringId2 = sut.StoreString("1234567890");
      //   Assert.IsTrue(stringId1 == stringId2);


      //}
   }
}
