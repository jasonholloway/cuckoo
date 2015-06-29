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
    public class StructTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckoosOnStructMethods() {
            var result = Tester.Static()
                                .Run(() => new Structs(123).GetNumber());

            Assert.IsTrue(result == 223);
        }

        [TestMethod]
        public void CuckooGetsStructInstance() {
            var number = Tester.Static()
                                  .Run(() => ((Structs)new Structs(123).GetInstance()).Number);

            Assert.IsTrue(number == 123);
        }


    }
}
