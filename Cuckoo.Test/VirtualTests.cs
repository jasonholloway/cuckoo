using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Weave;
using Cuckoo;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using System.Threading;

namespace Cuckoo.Test
{
    [TestClass]
    public class VirtualTests : WeaveTestBase
    {
        [TestMethod]
        public void CuckooOnVirtualMethod() {
            var result = Tester.With<Virtuals>().Run(v => v.VirtualMethod(99));

            Assert.IsTrue(result == 98665);
        }

        [TestMethod]
        public void OverridenMethodIgnoresCuckooOnBase() {
            var result = Tester.With<VirtualsDerived>().Run(v => v.VirtualMethod(99));
            
            Assert.IsTrue(result == 456);
        }

        [TestMethod]
        public void CuckooOnAbstractMethodIsIgnored() {
            var result = Tester.With<DerivedFromAbstractClass>()
                                .Run(d => d.AbstractMethod(123));

            Assert.IsTrue(result == 77);
        }

        [TestMethod]
        public void CuckooOnConcreteMethodInAbstractClass() {
            var result = Tester.With<DerivedFromAbstractClass>()
                                .Run(d => d.ConcreteMethod(399));

            Assert.IsTrue(result == 299);
        }
    }
}
