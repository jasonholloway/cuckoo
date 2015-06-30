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
    public class TypeSpecArgTests : WeavingTestBase
    {

        [TestMethod]
        public void ArrayArgs() {
            var callArgs = Tester.WithClass<TypeSpecArgs>()
                                .Run(t => t.ReturnGenericCallArgs<int[], string[]>(null, null));

            Assert.IsTrue(callArgs[0].Type == typeof(int[]));
            Assert.IsTrue(callArgs[1].Type == typeof(string[]));
        }

        [TestMethod]
        public void NullableArgs() {
            var callArgs = Tester.WithClass<TypeSpecArgs>()
                                .Run(t => t.ReturnGenericCallArgs<int?, float?>(null, null));

            Assert.IsTrue(callArgs[0].Type == typeof(int?));
            Assert.IsTrue(callArgs[1].Type == typeof(float?));
        }


    }
}
