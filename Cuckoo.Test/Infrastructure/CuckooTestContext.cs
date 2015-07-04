
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
        public static string TargetAssemblyPath { get; private set; }
        public static WeaverSandbox Sandbox { get; private set; }
        

        [AssemblyInitialize]
        public static void AsmInit(TestContext testContext) {            
            TargetAssemblyPath = Path.Combine(
                                        Environment.CurrentDirectory, 
                                        "Cuckoo.TestAssembly.dll" );

            Sandbox = new WeaverSandbox(TargetAssemblyPath);
        }

        [AssemblyCleanup]
        public static void AsmCleanUp() {
            ((IDisposable)CuckooTestContext.Sandbox).Dispose();
        }
    }


}
