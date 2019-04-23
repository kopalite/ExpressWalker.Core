using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class PropertyGuardTest
    {
        [TestMethod]
        public void PropertyGuard_FirstLevelCheck()
        {
            //Arrange

            var guard = new PropertyGuard();
            guard.Add(typeof(Node), "Child");
            guard.Add(typeof(Node), "Child");

            //Act

            var isRepeating = guard.IsRepeating(typeof(Node), "Child");

            //Assert

            Assert.IsTrue(isRepeating);
        }

        [TestMethod]
        public void PropertyGuard_SecondLevelCheck()
        {
            //Arrange

            var guard = new PropertyGuard();
            guard.Add(typeof(SimpleParent), "Child");
            guard.Add(typeof(SimpleChild), "Parent");
            guard.Add(typeof(SimpleParent), "Child");
            guard.Add(typeof(SimpleChild), "Parent");

            //Act

            var isRepeating = guard.IsRepeating(typeof(SimpleParent), "Child");

            //Assert

            Assert.IsTrue(isRepeating);
        }

        [TestMethod]
        public void PropertyGuard_ThirdLevelCheck()
        {
            //Arrange

            var guard = new PropertyGuard();
            guard.Add(typeof(First), "Second1");
            guard.Add(typeof(Second), "Third1");
            guard.Add(typeof(Third), "First1");
            guard.Add(typeof(First), "Second1");
            guard.Add(typeof(Second), "Third1");
            guard.Add(typeof(Third), "First1");

            //Act

            var isRepeating = guard.IsRepeating(typeof(First), "Second1");

            //Assert

            Assert.IsTrue(isRepeating);
        }
    }

    public class Node
    {
        public Node Child { get; set; }
    }

    public class SimpleParent
    {
        public SimpleChild Child { get; set; }
    }

    public class SimpleChild
    {
        public SimpleParent Parent { get; set; }
    }

    public class First
    {
        public Second Second1 { get; set; }
    }

    public class Second
    {
        public Third Third1 { get; set; }
    }

    public class Third
    {
        public First First1 { get; set; }
    }
}
