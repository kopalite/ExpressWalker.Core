using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressWalker.Core;
using ExpressWalker.Core.Visitors;
using System.Collections.Generic;
using System.Linq;

namespace ExpressWalker.Core.Test
{
    [TestClass]
    public class ManualWalkerTest
    {
        [TestMethod]
        public void ManualWalker_Visit()
        {
            //Arrange

            var sample = GetSample();

            //Act

            var walker = GetWalker();
            var blueprint = new A1();
            var values = new HashSet<PropertyValue>();
            walker.Visit(sample, blueprint, 10, new InstanceGuard(), values);

            //Assert

            Assert.IsTrue(IsCorrect(sample, blueprint, values));
        }

        private A1 GetSample()
        {
            return new A1
            {
                A1Date = DateTime.Now,
                A1Amount = 34,
                B1 = new B1
                {
                    B1Name = "TestB1",

                    C1 = new C1
                    {
                        C1Date = DateTime.Now
                    }
                },
                B2 = new B2
                {
                    B2Date = DateTime.Now,
                    IntArray = new[] { 1, 2, 3 },
                    C2List = new[] 
                    {
                        new C2 { C2Name = "aaa" },
                        new C2 { C2Name = "yyy" }
                    }
                }
            };
        }

        private IVisitor GetWalker()
        {
            return ManualWalker.Create<A1>()
                                    .Property<A1, DateTime>(a1 => a1.A1Date, (a1p, m) => a1p.AddYears(10))
                                    .Property<A1, int>(a1 => a1.A1Amount, (a1p, m) => a1p * 3)
                                    .Element<A1, B1>(a1 => a1.B1, b1 =>
                                            b1.Property<B1, string>(x => x.B1Name, (b1p, m) => b1p + "Test2")
/*nested elem.*/                               .Element<B1, C1>(b11 => b11.C1, c1 =>
/*nested prop.*/                                  c1.Property<C1, DateTime>(x => x.C1Date, (c1p, m) => c1p.AddYears(10))))
                                    .Element<A1, B2>(a1 => a1.B2, b2 => b2
                                        .Property<B2, DateTime>(x => x.B2Date, (b2p, m) => b2p.AddYears(10))
                                        .Property<B2, int[]>(x => x.IntArray, (b2p, m) => b2p.Select(x => x * 2).ToArray())
/*collection*/                          .Collection<B2, C2>(x => x.C2List, c2l =>
/*nested prop.*/                              c2l.Property<C2, string>(x => x.C2Name, (c2p, m) => c2p + "bbb")))
                                .Build();
        }

        
        private bool IsCorrect(A1 sample, A1 blueprint, HashSet<PropertyValue> values)
        {
            var tenYearsAfter = DateTime.Now.Year + 10;

            Func<A1, bool> isCorrect = a => a.A1Date.Year == tenYearsAfter &&
                   a.A1Amount == 102 &&
                   a.B1.B1Name == "TestB1Test2" &&
                   a.B1.C1.C1Date.Year == tenYearsAfter &&
                   a.B2.B2Date.Year == tenYearsAfter &&
                   a.B2.IntArray.Sum() == 1 * 2 + 2 * 2 + 3 * 2 &&
                   a.B2.C2List[0].C2Name == "aaabbb" &&
                   a.B2.C2List[1].C2Name == "yyybbb";

            return isCorrect(sample) && isCorrect(blueprint) && values.Count == 8;
        }

    }

    public class A1
    {
        public string A1Name { get; set; }

        public int A1Amount { get; set; }

        public DateTime A1Date { get; set; }

        public B1 B1 { get; set; }

        public B2 B2 { get; set; }
    }

    public class B1
    {
        public string B1Name { get; set; }

        public int B1Amount { get; set; }

        public DateTime B1Date { get; set; }

        public C1 C1 { get; set; }

        
    }

    public class B2
    {
        public string B2Name { get; set; }

        public int B2Amount { get; set; }

        public DateTime B2Date { get; set; }

        public int[] IntArray { get; set; }

        public C2[] C2List { get; set; }
    }

    public class C1
    {
        public string C1Name { get; set; }

        public int C1Amount { get; set; }

        public DateTime C1Date { get; set; }
    }

    public class C2
    {
        public string C2Name { get; set; }

        public int C2Amount { get; set; }

        public DateTime C2Date { get; set; }
    }
}
