using ExpressWalker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class ExpressAccessorTest
    {
        [TestMethod]
        public void ExpressAccessor_Getters()
        {
            //Arrange

            var dateTime = DateTime.Now;
            var childRef = new TestClass { TestString = "ChildRef" };
            var testObject = new TestClass { TestString = "bbb", TestInt = 10, TestDate = dateTime, TestRef = childRef };

            //Act

            var stringAcc = ExpressAccessor.Create(typeof(TestClass), typeof(string), "TestString");
            var intAcc = ExpressAccessor.Create(typeof(TestClass), typeof(int), "TestInt");
            var dateAcc = ExpressAccessor.Create(typeof(TestClass), typeof(DateTime), "TestDate");
            var refAcc = ExpressAccessor.Create(typeof(TestClass), typeof(TestClass), "TestRef");

            var @string = stringAcc.Get(testObject);
            var @int = intAcc.Get(testObject);
            var date = dateAcc.Get(testObject);
            var @ref = refAcc.Get(testObject);

            //Assert 

            Assert.AreEqual("bbb", @string);
            Assert.AreEqual(10, @int);
            Assert.AreEqual(dateTime, date);
            Assert.AreEqual(@ref, childRef);
        }

        [TestMethod]
        public void ExpressAccessor_Setters()
        {
            //Arrange

            var dateTime = DateTime.Now;
            var childRef = new TestClass { TestString = "ChildRef" };
            var testObject = new TestClass { TestString = "bbb", TestInt = 10, TestDate = dateTime, TestRef = null };

            //Act

            var stringAcc = ExpressAccessor.Create(typeof(TestClass), typeof(string), "TestString");
            var intAcc = ExpressAccessor.Create(typeof(TestClass), typeof(int), "TestInt");
            var dateAcc = ExpressAccessor.Create(typeof(TestClass), typeof(DateTime), "TestDate");
            var refAcc = ExpressAccessor.Create(typeof(TestClass), typeof(TestClass), "TestRef");

            stringAcc.Set(testObject, "aaa");
            intAcc.Set(testObject, 20);
            dateAcc.Set(testObject, DateTime.MaxValue);
            refAcc.Set(testObject, childRef);

            //Assert 

            Assert.AreEqual("aaa", testObject.TestString);
            Assert.AreEqual(20, testObject.TestInt);
            Assert.AreEqual(DateTime.MaxValue, testObject.TestDate);
            Assert.AreEqual(childRef, testObject.TestRef);
        }
    }

    public class TestClass
    {
        public string TestString { get; set; }

        public int TestInt { get; set; }

        public DateTime TestDate { get; set; }

        public TestClass TestRef { get; set; }
    }
}
