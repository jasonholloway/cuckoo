
using Cuckoo.Weave;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    [TestClass]
    public class CuckooTestContext
    {
        public static WeaverSandbox Sandbox { get; private set; }
        
        [AssemblyInitialize]
        public static void AsmInit(TestContext testContext) {            
            string asmPath = Path.Combine(Environment.CurrentDirectory, "Cuckoo.TestAssembly.dll");

            var weaver = new Cuckoo.Weave.Weaver();

            Sandbox = new WeaverSandbox(asmPath, weaver);

            Sandbox.Init();
        }

        [AssemblyCleanup]
        public static void AsmCleanUp() {
            ((IDisposable)CuckooTestContext.Sandbox).Dispose();
        }
    }


}
