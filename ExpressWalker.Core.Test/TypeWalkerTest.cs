using ExpressWalker.Core.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class TypeWalkerTest
    {
        private IVisitor _visitor;
        private Parent _sample;
        private Parent _blueprint;
        private HashSet<PropertyValue> _values;

        [TestInitialize]
        public void TestInitialize()
        {
            _visitor = TypeWalker<Parent>.Create()
                                     .ForProperty<Parent, int>(p => p.TestInt, (x, m) => x * x)
                                     .ForProperty<Parent, string>(p => p.TestString, (x, m) => x + x + m)
                                     .ForProperty<Child, DateTime>(p => p.TestDate1, (x, m) => x.AddYears(10))
                                     .ForProperty<CommonType>((x, m) => new CommonType { CommonString = "..." })
                                     .ForProperty<ClassLevel1, string>(p => p.SomeString, (x, m) => "visited").Build();
            _sample = new Parent
            {
                //level 0
                TestString = "aaa",
                TestInt = 10,
                TestDate = DateTime.Now,
                CommonType1 = new CommonType { CommonString = "brlj" },
                Child = new Child
                {
                    //level1
                    TestString1 = "aaa1",
                    TestInt1 = 10,
                    TestDate1 = DateTime.Now,
                    CommonType1 = new CommonType { CommonString = "njanja" },
                    ChildLevel1 = new ChildLevel1
                    {
                        //level2
                        ChildLevel2 = new ChildLevel2
                        {
                            TestCommonType = new CommonType { CommonString = "njunjnu" }
                        }
                    }
                },
                
                //level0
                CollectionChild = new CollectionChild()
                {
                    //level0
                    TestList = new List<ClassLevel1>
                    {
                        //level1
                        new ClassLevel1
                        {
                            //level2                       
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        },
                        new ClassLevel1
                        {
                            //level2    
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        }
                    },
                    //level0
                    TestCollection = new HashSet<ClassLevel1>(new[]
                    {
                        //level1
                        new ClassLevel1
                        {
                            //level2  
                            ClassLevel2 = new ClassLevel2 { TestCommonType = new CommonType { CommonString = "njunjnu" }},
                            SomeString = "njenje"
                        },
                        new ClassLevel1
                        {
                            //level2    
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        }
                    }),
                    //level0
                    TestIList = new List<ClassLevel1>
                    {
                        //level1
                        new ClassLevel1
                        {
                            //level2  
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" }},
                            SomeString = "njenje"
                        },
                        new ClassLevel1
                        {
                            //level2    
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        }
                    },
                    //level0
                    TestICollection = new HashSet<ClassLevel1>(new[]
                    {
                        //level1
                        new ClassLevel1
                        {
                            //level2  
                            ClassLevel2 = new ClassLevel2 {TestCommonType = new CommonType { CommonString = "njunjnu" }},
                            SomeString = "njenje"
                        },
                        new ClassLevel1
                        {
                            //level2    
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        }
                    }),
                    //level0
                    TestArray = new ClassLevel1[]
                    {
                        //level1
                        new ClassLevel1
                        {
                            //level2  
                            ClassLevel2 = new ClassLevel2 {TestCommonType = new CommonType { CommonString = "njunjnu" }},
                            SomeString = "njenje"
                        },
                        new ClassLevel1
                        {
                            //level2    
                            ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                            SomeString = "njenje"
                        }
                    }
                }, 

                //level0
                DictionaryChild = new DictionaryChild()
                {
                    //level0
                    TestDict = new Dictionary<int, ClassLevel1>()
                    {
                        {
                            //level1
                            1, new ClassLevel1
                            {
                                //level2                       
                                ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                                SomeString = "njenje"
                            }
                        }
                        ,
                        {
                            //level1
                            2, new ClassLevel1
                            {
                                //level2    
                                ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                                SomeString = "njenje"
                            }
                        }
                    },
                    //level0
                    TestIDict = new Dictionary<int, ClassLevel1>()
                    {
                        {
                            //level1
                            1, new ClassLevel1
                            {
                                //level2                       
                                ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                                SomeString = "njenje"
                            }
                        }
                        ,
                        {
                            //level1
                            2, new ClassLevel1
                            {
                                //level2    
                                ClassLevel2 = new ClassLevel2{ TestCommonType = new CommonType { CommonString = "njunjnu" } },
                                SomeString = "njenje"
                            }
                        }
                    }
                }
            };

            _sample.Child.Parent = _sample;

            _blueprint = new Parent();

            _values = new HashSet<PropertyValue>();
        }


        [TestMethod]
        public void TypeWalker_Visit_SimpleDict()
        {
            var bla = new Bla
            {
                Blas = new Dictionary<int, Bla1>
                {
                    { 1, new Bla1 { Test="t1" } },{ 2, new Bla1 { Test="t2" } }
                }
            };

            var visitor = TypeWalker<Bla>.Create().ForProperty<Bla1, string>(b1 => b1.Test, (v, m) => "visited").Build(1);

           
            visitor.Visit(bla, null, 1);

            Assert.IsTrue(bla.Blas[1].Test == "visited" && bla.Blas[2].Test == "visited");
        }

        public class Bla
        {
            public Dictionary<int, Bla1> Blas { get; set; }
        }

        public class Bla1
        {
            public string Test { get; set; }
        }

        [TestMethod]
        public void TypeWalker_Visit_All()
        {
            //Act

            _visitor.Visit(_sample, _blueprint, 10, new InstanceGuard(), _values);

            //Assert

            Func<IEnumerable<ClassLevel1>, bool> isCollectionOK = x =>
                   x.Count() == 2 &&
                   x.First().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.First().SomeString == "visited" &&
                   x.Last().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.Last().SomeString == "visited";

            Func<Parent, bool> isOk = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.ChildLevel1.ChildLevel2.TestCommonType.CommonString == "..." &&
                   isCollectionOK(p.CollectionChild.TestIList) &&
                   isCollectionOK(p.CollectionChild.TestICollection) &&
                   isCollectionOK(p.CollectionChild.TestList) &&
                   isCollectionOK(p.CollectionChild.TestCollection) &&
                   isCollectionOK(p.CollectionChild.TestArray);
            

            Assert.IsTrue(isOk(_sample) && isOk(_blueprint) && _values.Count == 26);
        }

        [TestMethod]
        public void TypeWalker_Visit_All_NullBlueprint()
        {
            //Act

            _visitor.Visit(_sample, null, 10, new InstanceGuard(), _values);

            //Assert

            Func<IEnumerable<ClassLevel1>, bool> isCollectionOK = x =>
                   x.Count() == 2 &&
                   x.First().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.First().SomeString == "visited" &&
                   x.Last().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.Last().SomeString == "visited";

            Func<Parent, bool> isOk = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.ChildLevel1.ChildLevel2.TestCommonType.CommonString == "..." &&
                   isCollectionOK(p.CollectionChild.TestIList) &&
                   isCollectionOK(p.CollectionChild.TestICollection) &&
                   isCollectionOK(p.CollectionChild.TestList) &&
                   isCollectionOK(p.CollectionChild.TestCollection) &&
                   isCollectionOK(p.CollectionChild.TestArray);


            Assert.IsTrue(isOk(_sample) && _values.Count == 26);
        }

        [TestMethod]
        public void TypeWalker_Visit_Depth3()
        {
            //Act

            _visitor.Visit(_sample, _blueprint, 3, new InstanceGuard(), _values);

            //Assert

            Func<IEnumerable<ClassLevel1>, bool> isCollectionOK = x =>
                   x.Count() == 2 &&
                   x.First().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.First().SomeString == "visited" &&
                   x.Last().ClassLevel2.TestCommonType.CommonString == "..." &&
                   x.Last().SomeString == "visited";

            Func<Parent, bool> isOk = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.ChildLevel1.ChildLevel2.TestCommonType.CommonString == "..." &&
                   isCollectionOK(p.CollectionChild.TestIList) &&
                   isCollectionOK(p.CollectionChild.TestICollection) &&
                   isCollectionOK(p.CollectionChild.TestList) &&
                   isCollectionOK(p.CollectionChild.TestCollection) &&
                   isCollectionOK(p.CollectionChild.TestArray);


            Assert.IsTrue(isOk(_sample) && _values.Count == 26);
        }

        [TestMethod]
        public void TypeWalker_Visit_Depth2_SampleOk()
        {
            //Act

            _visitor.Visit(_sample, _blueprint, 2, new InstanceGuard(), _values);

            //Assert

            Func<IEnumerable<ClassLevel1>, bool> isCollectionOK = x =>
                   x.Count() == 2 &&
                   x.First().ClassLevel2.TestCommonType.CommonString == "njunjnu" && //this property is on level 2 / depth 3, should not be touched
                   x.First().SomeString == "visited" &&
                   x.Last().ClassLevel2.TestCommonType.CommonString == "njunjnu" && //this property is on level 2 / depth 3, should not be touched
                   x.Last().SomeString == "visited";

            Func<Parent, bool> isOk = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.ChildLevel1.ChildLevel2.TestCommonType.CommonString == "njunjnu" && //this property is on level 2 / depth 3, should not be touched
                   isCollectionOK(p.CollectionChild.TestIList) &&
                   isCollectionOK(p.CollectionChild.TestICollection) &&
                   isCollectionOK(p.CollectionChild.TestList) &&
                   isCollectionOK(p.CollectionChild.TestCollection) &&
                   isCollectionOK(p.CollectionChild.TestArray);


            Assert.IsTrue(isOk(_sample) && _values.Count == 15);
        }

        [TestMethod]
        public void TypeWalker_Visit_Depth2_BlueprintOk()
        {
            //Act

            _visitor.Visit(_sample, _blueprint, 2, new InstanceGuard(), _values);

            //Assert

            Func<IEnumerable<ClassLevel1>, bool> isCollectionOK = x =>
                   x.Count() == 2 &&
                   x.First().ClassLevel2.TestCommonType == null && //this property is on level 2 / depth 3, should not be blueprinted, as original was not visited
                   x.First().SomeString == "visited" &&
                   x.Last().ClassLevel2.TestCommonType == null && //this property is on level 2 / depth 3, should not be touched, as original was not visited
                   x.Last().SomeString == "visited";

            Func<Parent, bool> isOk = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.ChildLevel1.ChildLevel2.TestCommonType == null && //this property is on level 2 / depth 3, should not be touched, as original was not visited
                   isCollectionOK(p.CollectionChild.TestIList) &&
                   isCollectionOK(p.CollectionChild.TestICollection) &&
                   isCollectionOK(p.CollectionChild.TestList) &&
                   isCollectionOK(p.CollectionChild.TestCollection) &&
                   isCollectionOK(p.CollectionChild.TestArray);


            Assert.IsTrue(isOk(_blueprint) && _values.Count == 15);
        }

        [TestMethod]
        //TODO: Examine why there is no exception: [ExpectedException(typeof(OutOfMemoryException))]
        public void TypeWalker_Visit_All_NoInstanceGuard()
        {
            //Act

            _visitor.Visit(_sample, blueprint: _blueprint, guard: null, values: _values);
        }

        [TestMethod]

        public void TypeWalker_Visit_All_NoValues()
        {
            //Act

            _visitor.Visit(_sample, _blueprint, 10, new InstanceGuard(), null);

            //Assert (Since we came here without values parameter, test is passed).
        }
    }

    public class Parent
    {
        [VisitorMetadata("metadata")]
        public string TestString { get; set; }

        public int TestInt { get; set; }

        public DateTime TestDate { get; set; }

        public virtual Child Child { get; set; }

        public CommonType CommonType1 { get; set; }

        public CollectionChild CollectionChild { get; set; }

        public DictionaryChild DictionaryChild { get; set; }
    }

    public class Child
    {
        public string TestString1 { get; set; }

        public int TestInt1 { get; set; }

        public DateTime TestDate1 { get; set; }

        //For testing circular references.

        public virtual Parent Parent { get; set; }

        public CommonType CommonType1 { get; set; }

        public EnumTest Enum1 { get; set; }

        public EnumTest? Enum2 { get; set; }

        public ChildLevel1 ChildLevel1 { get; set; }
    }

    public class ChildLevel1
    {
        public ChildLevel2 ChildLevel2 { get; set; }
    }

    public class ChildLevel2
    { 
        public CommonType TestCommonType { get; set; }
    }



    public class CollectionChild
    {
        public IList<ClassLevel1> TestIList { get; set; }

        public ICollection<ClassLevel1> TestICollection { get; set; }

        public List<ClassLevel1> TestList { get; set; }

        public ICollection<ClassLevel1> TestCollection { get; set; }

        public ClassLevel1[]  TestArray{ get; set; }

        public object ReadOnly { get; }
    }

    public class DictionaryChild
    {
        public IDictionary<int, ClassLevel1> TestIDict { get; set; }

        public Dictionary<int, ClassLevel1> TestDict { get; set; }
    }

    public class ClassLevel1
    {
        public string SomeString { get; set; }

        public ClassLevel2 ClassLevel2 { get; set; }
    }

    public class ClassLevel2
    {
        public CommonType TestCommonType { get; set; }
    }

    public class CommonType
    {
        public string CommonString { get; set; }
    }

    public enum EnumTest
    {
        First = 1,
        Second = 2
    }
}
