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
    public class StructTests : WeavingTestBase2
    {

        [TestMethod]
        public void CuckoosOnStructMethods() {
            var result = Tester.With<StructRunner>()
                                  .Run(s => s.GetNumber(123));

            Assert.IsTrue(result == 223);
        }

        [TestMethod]
        public void CuckooGetsStructInstance() {
            var number = Tester.With<StructRunner>()
                                .Run(s => ((TestStruct)s.GetInstance(123))._number);

            Assert.IsTrue(number == 123);
        }

        [TestMethod]
        public void CuckooAffectsOriginalStruct() {
            var number = Tester.With<StructRunner>()
                                .Run(s => s.SetAndRetrieveNumber(8));

            Assert.IsTrue(number == 8);
        }



    }
}
