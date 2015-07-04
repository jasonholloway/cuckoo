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
    public class ArgTests : WeaveTestBase
    {

        [TestMethod]
        public void CuckooChangesArgs() {
            var result = Tester.With<Args>()
                                .Run(a => a.MethodReturnsStrings(1, "", "", 1f, ""));

            Assert.IsTrue(result == "Growl! Growl! Growl!");
        }
        

        [TestMethod]
        public void CuckooAllowsOutArg() {
            int x = 0;
            string y = "";

            var result = Tester.With<Args>()
                                .Run(a => a.MethodWithOutArg(1, out x, out y));

            Assert.IsTrue(x == 666);
            Assert.IsTrue(y == "Surprise!");
            Assert.IsTrue(result == "hello");
        }


        [TestMethod]
        public void TieredCuckoosAllowOutArg() {
            int x = 0;
            string y = "";

            var result = Tester.With<Args>()
                                .Run(a => a.MethodWithOutArgAndManyCuckoos(1, out x, out y));

            Assert.IsTrue(x == 666);
            Assert.IsTrue(y == "Surprise!");
            Assert.IsTrue(result == "hello");
        }
        

        [TestMethod]
        public void CuckooAllowsRefArg() {
            string s = "";
            int i = 0;

            var result = Tester.With<Args>()
                                  .Run(a => a.MethodWithRefArg(ref s, ref i));

            Assert.IsTrue(result == "yup");
            Assert.IsTrue(s == "dreariment");
            Assert.IsTrue(i == 999);
        }


        [TestMethod]
        public void CuckooChangesOutArg() {
            string s = "";
            int i = 0;

            var result = Tester.With<Args>()
                                  .Run(a => a.MethodWithChangedRefArgs(ref s, ref i));

            Assert.IsTrue(result == "yup");
            Assert.IsTrue(s == "glug");
            Assert.IsTrue(i == 666);
        }

        [TestMethod]
        public void CuckoosAllowOptionalArgs() {            
            var result = Tester.With<Args>()
                                    .Run(a => a.MethodWithOptionalArgs(1));

            Assert.IsTrue(result == "13");
        }






    }
}
