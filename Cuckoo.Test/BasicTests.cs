using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cuckoo.Test
{
    [TestClass]
    public class BasicTests : WeavingTestBase
    {

        [TestMethod]
        public void SimpleCall() {
            Tester.WithClass<Basic>().Run(b => b.SimpleMethod());
        }


        [TestMethod]
        public void SimpleCallWithReturn() {
            int result = Tester.WithClass<Basic>().Run(b => b.SimpleMethodWithReturn());

            Assert.IsTrue(result == 13);
        }


        //[TestMethod]
        //public void RoostsInPlace() {
        //    foreach(var method in UsurpedMethods) {
        //        var fRoost = method.DeclaringType.GetField(
        //                                            "<ROOST>_" + method.Name, 
        //                                            BindingFlags.Static | BindingFlags.NonPublic);

        //        object value = fRoost.GetValue(null);
        //        Assert.IsTrue(value is IRoost);
                
        //        var roost = value as IRoost;
        //        Assert.AreEqual(roost.Method, method);
        //        Assert.IsTrue(roost.Cuckoos is ICuckoo[]);
        //        Assert.IsTrue(roost.Cuckoos.All(u => u is CuckooAttribute));
        //    }
        //}

        [TestMethod]
        public void CuckooReturnsValue() {            
            var result = Tester.WithClass<Basic>()
                                .Run(b => b.MethodReturnsString());

            Assert.IsTrue(result == "Hello from down below!");
        }


        [TestMethod]
        public void CuckooChangesReturnValue() {
            string result = Tester.WithClass<Basic>()
                                    .Run(b => b.MethodWithChangeableReturn());

            Assert.IsTrue(result == "CHANGED!");
        }


        [TestMethod]
        public void CuckoosWorkTogether() {
            int result = Tester.WithClass<Basic>()
                                .Run(b => b.MethodReturnsInt());
            
            Assert.IsTrue(result == 13 + 8 - 10);
        }

        [TestMethod]
        public void CuckoosWorkTogetherInCorrectOrder() {
            string result = Tester.WithClass<Basic>()
                                    .Run(b => b.MethodWithTwoCuckoos("blah", 123));
            
            Assert.IsTrue(result == "BLAH");
        }

        [TestMethod]
        public void CuckooOnVoidMethod() {
            Tester.WithClass<Basic>().Run(b => b.VoidMethod());
        }

        [TestMethod]
        public void ImportedCuckooOnMethod() {
            var result = Tester.WithClass<Basic>().Run(b => b.MethodWithDistantCuckoo());

            Assert.IsTrue(result == 999);
        }


    }
}
