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
using System.Threading;

namespace Cuckoo.Test
{
    [TestClass]
    public class AsyncTests : WeaveTestBase
    {
        [TestMethod]
        public void CuckooOnVoidAsyncMethod() {
            Tester.With<Async>().Run(a => a.VoidAsyncMethod());
            Thread.Sleep(500);
        }

        [TestMethod]
        public void CuckooOnReturningAsyncMethod() {
            var result = Tester.With<Async>()
                                .Run(a => a.IntAsyncMethodRunner());
            
            Assert.IsTrue(result == 123);
        }

    }
}
