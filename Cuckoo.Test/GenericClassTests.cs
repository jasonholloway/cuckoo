using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test
{
    [TestClass]
    public class GenericClassTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooOnMethodInGenericClass() {
            int result = Tester.WithClass<GenericClass<int, int>>()
                                .Run(c => c.MethodInGenericClass(123));

            Assert.IsTrue(result == 12345);
        }


        [TestMethod]
        public void CuckooOnMethodWithGenArgsInGenClass() {
            int result = Tester.WithClass<GenericClass<int, int>>()
                                .Run(c => c.MethodWithGenericArgsInGenericClass<string, string>("a", "b"));

            Assert.IsTrue(result == 887);
        }


    }
}
