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

namespace Cuckoo.Test
{
    [TestClass]
    public class StaticTests : WeaveTestBase
    {
        [TestMethod]
        public void CuckooOnStaticMethod() {            
            var result = Tester.With<StaticRunner>()
                                .Run(r => r.StaticMethodInInstanceClass(10, 10));

            Assert.IsTrue(result == 40);
        }

        [TestMethod]
        public void CuckooOnStaticMethodInStaticClass() {
            var result = Tester.With<StaticRunner>()
                                .Run(r => r.StaticMethodInStaticClass(20));

            Assert.IsTrue(result == 40);
        }

        [TestMethod]
        public void CuckooOnExtensionMethod() {
            var result = Tester.With<StaticRunner>()
                                .Run(r => r.ExtensionMethod(7));

            Assert.IsTrue(result == 407);
        }
    }


}
