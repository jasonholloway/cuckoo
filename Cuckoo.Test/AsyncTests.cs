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
using System.Threading;

namespace Cuckoo.Test
{
    [TestClass]
    public class AsyncTests : WeavingTestBase
    {
        [TestMethod]
        public void CuckooOnVoidAsyncMethod() {
            Tester.WithClass<Async>().Run(a => a.VoidAsyncMethod());
            Thread.Sleep(500);
        }

        [TestMethod]
        public void CuckooOnReturningAsyncMethod() {
            var result = Tester.WithClass<Async>().Run(a => a.IntAsyncMethod()).Result;
            
            Assert.IsTrue(result == 123);
        }

    }
}
