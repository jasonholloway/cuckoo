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
    public class GenericMethodTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooOnMethodWithGenericArgs() {
            var method = GetUsurpedMethod("MethodWithGenericArgs");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 999);
        }


        [TestMethod]
        public void CuckooOnMethodWithGenericResult() {
            var method = GetUsurpedMethod("MethodWithGenericResult");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == default(string));
        }

        [TestMethod]
        public void CuckoosCooperateOnMethodWithGenericArgs() {
            var method = GetUsurpedMethod("TreblyCuckooedMethodWithGenericArgs");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 90);
        }


    }
}
