using Cuckoo.Test.Infrastructure;
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
            var method = GetUsurpedMethod("MethodInGenericClass");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 12345);
        }


        [TestMethod]
        public void CuckooOnMethodWithGenArgsInGenClass() {
            var method = GetUsurpedMethod("MethodWithGenericArgsInGenericClass");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 887);
        }


    }
}
