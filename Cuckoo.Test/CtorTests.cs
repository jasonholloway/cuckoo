using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Cuckoo.Test
{
    [TestClass]
    public class CtorTests : WeaveTestBase
    {

        [TestMethod]
        public void CuckoosOnCtor() {
            var result = Tester.With<CtorRunner>()
                                    .Run(r => r.CtorWithBaseCalculation(12, 12).BaseValue);

            Assert.IsTrue(result == 7700);

            result = Tester.With<CtorRunner>()
                                .Run(r => r.CtorWithBaseCalculation(12, 12).DerivedValue);

            Assert.IsTrue(result == 99);
        }


        [TestMethod]
        public void CuckooOnCtorGetsInstance() {
            Tester.With<CtorRunner>()
                      .Run(r => r.Ctor(1, 2, 3).DerivedValue);
        }

    }
}
