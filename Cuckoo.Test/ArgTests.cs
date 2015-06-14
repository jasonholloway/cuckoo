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
    public class ArgTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooChangesArgs() {
            var method = GetUsurpedMethod("MethodReturnsStrings");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Growl! Growl! Growl!");
        }



        [TestMethod]
        public void CuckooChangesOutArg() {
            //...
        }


        [TestMethod]
        public void CuckooChangesRefArg() {
            //...
        }



    }
}
