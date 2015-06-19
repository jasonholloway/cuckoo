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
using System.Linq.Expressions;
using Cuckoo.Impl;
using Cuckoo.Attributes;

namespace Cuckoo.Test
{
    [TestClass]
    public class BasicTests : WeavingTestBase
    {
        /*
        [TestMethod]
        public void AllUsurpationsCallable() {
            Assert.IsTrue(UsurpedMethods.Any(), "No usurpations!");

            foreach(var method in UsurpedMethods) {
                System.Diagnostics.Debug.WriteLine(method.Name);
                MethodTester.Test(method); 
            }
        }*/


        [TestMethod]
        public void RoostsInPlace() {
            foreach(var method in UsurpedMethods) {
                var fRoost = method.DeclaringType.GetField(
                                                    "<ROOST>_" + method.Name, 
                                                    BindingFlags.Static | BindingFlags.NonPublic);

                object value = fRoost.GetValue(null);
                Assert.IsTrue(value is Roost);

                var roost = value as Roost;
                Assert.AreEqual(roost.Method, method);
                Assert.IsTrue(roost.Cuckoos is ICuckoo[]);
                Assert.IsTrue(roost.Cuckoos.All(u => u is CuckooAttribute));
            }
        }

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
        public void CuckoosOnCtor() {
            var result = Tester.Static()
                                    .Run(() => new Basic.CtorClass(12, 12).BaseValue );

            Assert.IsTrue(result == 13);

            result = Tester.Static()
                                .Run(() => new Basic.CtorClass(12, 12).DerivedValue);

            Assert.IsTrue(result == 13);
        }




    }
}
