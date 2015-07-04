using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cuckoo.Test
{
    [TestClass]
    public class BasicTests : WeaveTestBase
    {

        [TestMethod]
        public void SimpleCall() {
            Tester.With<Basic>().Run(b => b.SimpleMethod());
        }


        [TestMethod]
        public void SimpleCallWithReturn() {
            int result = Tester.With<Basic>().Run(b => b.SimpleMethodWithReturn());

            Assert.IsTrue(result == 13);
        }



        [TestMethod]
        public void CuckooReturnsValue() {            
            var result = Tester.With<Basic>()
                                .Run(b => b.MethodReturnsString());

            Assert.IsTrue(result == "Hello from down below!");
        }


        [TestMethod]
        public void CuckooChangesReturnValue() {
            string result = Tester.With<Basic>()
                                    .Run(b => b.MethodWithChangeableReturn());

            Assert.IsTrue(result == "CHANGED!");
        }


        [TestMethod]
        public void CuckoosWorkTogether() {
            int result = Tester.With<Basic>()
                                .Run(b => b.MethodReturnsInt());
            
            Assert.IsTrue(result == 13 + 8 - 10);
        }

        [TestMethod]
        public void CuckoosWorkTogetherInCorrectOrder() {
            string result = Tester.With<Basic>()
                                    .Run(b => b.MethodWithTwoCuckoos("blah", 123));
            
            Assert.IsTrue(result == "BLAH");
        }

        [TestMethod]
        public void CuckooOnVoidMethod() {
            Tester.With<Basic>().Run(b => b.VoidMethod());
        }

        [TestMethod]
        public void CuckooFromOtherAssembly() {
            var result = Tester.With<Basic>()
                                .Run(b => b.MethodWithDistantCuckoo());

            Assert.IsTrue(result == 999);
        }

    }
}
