using Cuckoo;
using Cuckoo.Weave;
using Cuckoo.Impl;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Sequences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cuckoo.Test.Infrastructure
{
    public abstract class WeaveTestBase : IDisposable
    {
        protected string TargetAssemblyPath { get; private set; }
        protected WeaverSandbox Sandbox { get; private set; }
        protected IMethodTester Tester { get; private set; }

        protected WeaveTestBase() {
            TargetAssemblyPath = Path.Combine(
                                        Environment.CurrentDirectory,
                                        "Cuckoo.TestAssembly.dll");

            Sandbox = new WeaverSandbox(TargetAssemblyPath, "CuckooWeave1");

            var roostSpecs = Sandbox.Gather();

            Sandbox.Weave(roostSpecs);

            Tester = new MethodTester2(Sandbox);
        }

        public void Dispose() {
            ((IDisposable)Sandbox).Dispose();
        }

    }
}
