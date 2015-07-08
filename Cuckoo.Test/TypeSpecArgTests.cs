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
    public class TypeSpecArgTests : WeaveTestBase
    {

        [TestMethod]
        public void ArrayArgs() {
            var callArgTypeNames = Tester.With<TypeSpecArgs>()
                                             .Run(t => t.MethodWithArrayArgs(new int[0], new string[0], new double[0]));

            Assert.IsTrue(callArgTypeNames[0] == typeof(int[]).FullName);
            Assert.IsTrue(callArgTypeNames[1] == typeof(string[]).FullName);
        }

        [TestMethod]
        public void NullableArgs() {
            var callArgTypeNames = Tester.With<TypeSpecArgs>()
                                            .Run(t => t.MethodWithNullableArgs(9, 5F, 13UL));

            Assert.IsTrue(callArgTypeNames[0] == typeof(int?).FullName);
            Assert.IsTrue(callArgTypeNames[1] == typeof(float?).FullName);
        }


    }
}
