using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Fody;
using Cuckoo.Common;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;

namespace Cuckoo.Test
{
    [TestClass]
    public class BasicTests : WeavingTestBase
    {

        [TestMethod]
        public void AllUsurpationsCallable() {
            Assert.IsTrue(UsurpedMethods.Any(), "No usurpations!");

            foreach(var method in UsurpedMethods) {
                System.Diagnostics.Debug.WriteLine(method.Name);
                MethodTester.Test(method); 
            }
        }


        [TestMethod]
        public void CallSitesInPlace() {
            foreach(var method in UsurpedMethods) {
                var fCallSite = method.DeclaringType.GetField(
                                                        "<CALLSITE>_" + method.Name, 
                                                        BindingFlags.Static | BindingFlags.NonPublic);

                object value = fCallSite.GetValue(null);
                Assert.IsTrue(value is Cuckoo.Common.CallSite);

                var callSite = value as Cuckoo.Common.CallSite;
                Assert.AreEqual(callSite.Method, method);
                Assert.IsTrue(callSite.Usurpers is ICallUsurper[]);
                Assert.IsTrue(callSite.Usurpers.All(u => u is CuckooAttribute));
            }
        }

        [TestMethod]
        public void CuckooReturnsValue() {
            var method = GetUsurpedMethod("MethodReturnsString");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Hello from down below!");
        }


        [TestMethod]
        public void CuckooChangesReturnValue() {
            var method = GetUsurpedMethod("MethodWithChangeableReturn");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "CHANGED!");
        }


        [TestMethod]
        public void CuckoosWorkTogether() {
            var method = GetUsurpedMethod("MethodReturnsInt");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 13 + 8 - 10);
        }

        [TestMethod]
        public void CuckoosWorkTogetherInCorrectOrder() {
            var method = GetUsurpedMethod("MethodWithTwoCuckoos");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Wow!");
        }



        [TestMethod]
        public void CuckooChangesArgs() {
            var method = GetUsurpedMethod("MethodReturnsStrings");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Growl! Growl! Growl!");
        }





    }
}
