using ExpressWalker.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class UtilTest
    {
        struct TestStruct
        {
            public string Prop1;
            public int Prop2;
        }

        class TestClass1
        {
            public string Prop1;
            public int Prop2;
        }

        enum TestEnum
        {
            First = 1,
            Second = 2
        }

        [TestMethod]
        public void Util_IsSimpleType()
        {
            Assert.IsTrue(Util.IsSimpleType(typeof(Enum)));
            Assert.IsTrue(Util.IsSimpleType(typeof(TestEnum)));
            Assert.IsTrue(Util.IsSimpleType(typeof(TestEnum?)));


            Assert.IsTrue(Util.IsSimpleType(typeof(String)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Char)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Guid)));

            Assert.IsTrue(Util.IsSimpleType(typeof(Boolean)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Byte)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Int16)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Int32)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Int64)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Single)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Double)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Decimal)));

            Assert.IsTrue(Util.IsSimpleType(typeof(SByte)));
            Assert.IsTrue(Util.IsSimpleType(typeof(UInt16)));
            Assert.IsTrue(Util.IsSimpleType(typeof(UInt32)));
            Assert.IsTrue(Util.IsSimpleType(typeof(UInt64)));

            Assert.IsTrue(Util.IsSimpleType(typeof(DateTime)));
            Assert.IsTrue(Util.IsSimpleType(typeof(DateTimeOffset)));
            Assert.IsTrue(Util.IsSimpleType(typeof(TimeSpan)));

            Assert.IsFalse(Util.IsSimpleType(typeof(TestStruct)));
            Assert.IsFalse(Util.IsSimpleType(typeof(TestClass1)));

            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Char>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Guid>)));

            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Boolean>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Byte>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Int16>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Int32>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Int64>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Single>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Double>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<Decimal>)));
                          
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<SByte>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<UInt16>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<UInt32>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<UInt64>)));
                          
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<DateTime>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<DateTimeOffset>)));
            Assert.IsTrue(Util.IsSimpleType(typeof(Nullable<TimeSpan>)));

            Assert.IsFalse(Util.IsSimpleType(typeof(Nullable<TestStruct>)));
        }
    }
}
