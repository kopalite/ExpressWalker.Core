using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressWalker.Core.Factories;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class VisitorsFactoryTest
    {
        private IVisitorsFactory _factory;

        [TestInitialize]
        public void Initialize()
        {
            _factory = new VisitorsFactory().WithSettings("default")
                                                .ForProperty<int>((val, met) => 2)
                                            .WithSettings("name2")
                                                .ForProperty<ForFact2, string>( x => x.Name, (val, met) => "visited");
        }

        [TestMethod]
        public void Factory_Name_Default()
        {
            //Act
            var defaultVisitor = _factory.GetVisitor("default", typeof(ForFact1));

            //Assert
            Assert.IsNotNull(defaultVisitor);
        }

        [TestMethod]
        public void Factory_Name_Name2()
        {
            //Act
            var defaultVisitor = _factory.GetVisitor("name2", typeof(ForFact2));

            //Assert
            Assert.IsNotNull(defaultVisitor);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public void Factory_Name_Unknown()
        {
            //Act
            var defaultVisitor = _factory.GetVisitor("name6", typeof(ForFact2));

            //Assert
            Assert.IsNotNull(defaultVisitor);
        }

        [TestMethod]
        public void Factory_Visitor_Default()
        {
            //Arrange
            var defaultVisitor = _factory.GetVisitor("default", typeof(ForFact1));
            var x = new ForFact1 { Id = 1, Fact2 = new ForFact2() };

            //act
            defaultVisitor.Visit(x);


            //Assert
            Assert.IsNotNull(x.Id == 2 && x.Fact2.Name == null);
        }

        [TestMethod]
        public void Factory_Visitor_Name2()
        {
            //Arrange
            var cat2Visitor = _factory.GetVisitor("name2", typeof(ForFact1));
            var x = new ForFact1 { Id = 1, Fact2 = new ForFact2 { Name = "..." } };

            //act
            cat2Visitor.Visit(x);


            //Assert
            Assert.IsTrue(x.Id == 1 && x.Fact2.Name == "visited");
        }

        [TestMethod]
        public void Factory_Visitor_Cache()
        {
            //act
            var cat2Visitor1 = _factory.GetVisitor("name2", typeof(ForFact1));
            var cat2Visitor2 = _factory.GetVisitor("name2", typeof(ForFact1));
            
            //Assert
            Assert.IsTrue(cat2Visitor1 == cat2Visitor2);
        }
    }

    public class ForFact1
    {
        public int Id { get; set; }

        public ForFact2 Fact2 { get; set; }
    }

    public class ForFact2
    {
        public string Name { get; set; }
    }
}
