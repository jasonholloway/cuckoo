using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Fody;
using Cuckoo;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using System.Threading;

namespace Cuckoo.Test
{
    [TestClass]
    public class VirtualTests : WeavingTestBase
    {
        [TestMethod]
        public void CuckooOnVirtualMethod() {
            var result = Tester.WithClass<Virtuals>().Run(v => v.VirtualMethod(99));

            Assert.IsTrue(result == 98665);
        }

        [TestMethod]
        public void OverridenMethodIgnoresCuckooOnBase() {
            var result = Tester.WithClass<VirtualsDerived>().Run(v => v.VirtualMethod(99));
            
            Assert.IsTrue(result == 456);
        }

        [TestMethod]
        public void CuckooOnAbstractMethodIsIgnored() {
            var result = Tester.WithClass<DerivedFromAbstractClass>()
                                .Run(d => d.AbstractMethod(123));

            Assert.IsTrue(result == 77);
        }
    }
}
