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

namespace Cuckoo.Test
{
    [TestClass]
    public class StaticTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooOnStaticMethod() {
            var result = Tester.Static()
                                .Run(() => StaticMethods.StaticMethod(10, 10));

            Assert.IsTrue(result == 40);
        }

        [TestMethod]
        public void CuckooOnStaticMethodInStaticClass() {
            var result = Tester.Static()
                                .Run(() => StaticClass.StaticMethodInStaticClass(20));

            Assert.IsTrue(result == 40);
        }

        [TestMethod]
        public void CuckooOnExtensionMethod() {
            var result = Tester.WithClass<StaticMethods>()
                                .Run(s => s.ExtensionMethod(7));

            Assert.IsTrue(result == 407);
        }



    }


}
