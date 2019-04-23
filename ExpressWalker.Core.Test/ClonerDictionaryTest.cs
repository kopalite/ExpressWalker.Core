using ExpressWalker.Core.Cloners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class ClonerDictionaryTest
    {
        [TestMethod]
        public void Cloner_Dictionary_Dict()
        {
            //Arrange

            IDictionary<int, Test3> test3Dict = new Dictionary<int, Test3>()
            {
                { 1,  new Test3 { Name = "Name11" } },
                { 2,  new Test3 { Name = "Name12"} } 
            };
            

            //Act

            var cloner = ClonerBase.Create(test3Dict.GetType());
            var clone = (Dictionary<int, Test3>)cloner.Clone(test3Dict);

            //Assert

            Assert.IsTrue(clone != null && 
                          clone != test3Dict &&
                          clone.GetType() == test3Dict.GetType() &&
                          clone.Count == 2 && 
                          clone[1].Name == "Name11" &&
                          clone[2].Name == "Name12");
        }

        [TestMethod]
        public void Cloner_Dictionary_IDict()
        {
            //Arrange

            IDictionary<int, Test3> test3Dict = new Dictionary<int, Test3>()
            {
                { 1,  new Test3 { Name = "Name11" } },
                { 2,  new Test3 { Name = "Name12"} }
            };


            //Act

            var cloner = ClonerBase.Create(typeof(IDictionary<int, Test3>));
            var clone = (Dictionary<int, Test3>)cloner.Clone(test3Dict);

            //Assert

            Assert.IsTrue(clone != null &&
                          clone != test3Dict &&
                          clone.GetType() == test3Dict.GetType() &&
                          clone.Count == 2 &&
                          clone[1].Name == "Name11" &&
                          clone[2].Name == "Name12");
        }
    }

    public class Test3
    {
        public string Name { get; set; }
    }
}
