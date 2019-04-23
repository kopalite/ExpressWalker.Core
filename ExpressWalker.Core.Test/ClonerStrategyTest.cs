using ExpressWalker.Core.Cloners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class ClonerStrategyTest
    {
        [TestMethod]
        public void Cloner_Strategy_List()
        {
            //Arange

            var strategy = new ListStrategy();

            //Act

            var result1 = strategy.IsMatch(typeof(List<Class1>));
            var result2 = strategy.IsMatch(typeof(HashSet<Class1>));
            

            //Assert

            Assert.IsTrue(result1, "ListStrategy should be able to make cloner for type List<Class1>");
            Assert.IsTrue(result2, "ListStrategy should be able to make cloner for type HashSet<Class1>");
            
        }

        [TestMethod]
        public void Cloner_Strategy_IList()
        {
            //Arange

            var strategy = new ListInterfaceStrategy();

            //Act

            var result = strategy.IsMatch(typeof(IList<Class1>));
            

            //Assert

            Assert.IsTrue(result, "ListInterfaceStrategy should be able to make cloner for type IList<Class1>");
            
        }


        [TestMethod]
        public void Cloner_Strategy_Collection()
        {
            //Arange

            var strategy = new CollectionStrategy();

            //Act

            var result = strategy.IsMatch(typeof(Collection<Class1>));
            

            //Assert

            Assert.IsTrue(result, "CollectionStrategy should be able to make cloner for type Collection<Class1>");
        }

        [TestMethod]
        public void Cloner_Strategy_ICollection()
        {
            //Arange

            var strategy = new CollectionInterfaceStrategy();

            //Act

            var result = strategy.IsMatch(typeof(ICollection<Class1>));

            //Assert

            Assert.IsTrue(result, "CollectionInterfaceStrategy should be able to make cloner for type ICollection<Class1>");
        }

        [TestMethod]
        public void Cloner_Strategy_ArrayList()
        {
            //Arange

            var strategy = new ArrayListStrategy();

            //Act

            var result = strategy.IsMatch(typeof(ArrayList));


            //Assert

            Assert.IsTrue(result, "ArrayListStrategy should be able to make cloner for type ArrayList");
        }

        [TestMethod]
        public void Cloner_Strategy_Array()
        {
            //Arange

            var strategy = new ArrayStrategy();

            //Act

            var result = strategy.IsMatch(typeof(Class1[]));

            //Assert

            Assert.IsTrue(result, "ArrayStrategy should be able to make cloner for type Class1[]");
        }
    }

    public class Class1
    {

    }
}
