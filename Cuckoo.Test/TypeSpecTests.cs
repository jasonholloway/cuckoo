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
                                .Run(t => t.MethodWithArrayArgs(new int[0], new string[0], new double[0]));

            Assert.IsTrue(callArgs[0].Type == typeof(int[]));
            Assert.IsTrue(callArgs[0] is ICallArg<int[]>);

            Assert.IsTrue(callArgs[1].Type == typeof(string[]));
            Assert.IsTrue(callArgs[1] is ICallArg<string[]>);
        }

        [TestMethod]
        public void NullableArgs() {
            var callArgs = Tester.WithClass<TypeSpecArgs>()
                                .Run(t => t.MethodWithNullableArgs(9, 5F, 13UL));

            Assert.IsTrue(callArgs[0].Type == typeof(int?));
            Assert.IsTrue(callArgs[0] is ICallArg<int?>);

            Assert.IsTrue(callArgs[1].Type == typeof(float?));
            Assert.IsTrue(callArgs[1] is ICallArg<float?>);
        }


    }
}
