using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cuckoo.Test
{
    [TestClass]
    public class CtorTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckoosOnCtor() {
            var result = Tester.Static()
                                    .Run(() => new CtorClass(12, 12).BaseValue);

            Assert.IsTrue(result == 7700);

            result = Tester.Static()
                                .Run(() => new CtorClass(12, 12).DerivedValue);

            Assert.IsTrue(result == 99);
        }


        [TestMethod]
        public void CuckooOnCtorGetsInstance() {
            Tester.Static()
                      .Run(() => new CtorClass(1, 2, 3).DerivedValue);
        }

    }
}
