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

namespace Cuckoo.Test.Infrastructure
{
    public abstract class WeavingTestBase2 : IDisposable
    {
        protected IMethodTester Tester { get; private set; }

        public WeavingTestBase2() {
            this.Tester = new MethodTester2(CuckooTestContext.Sandbox);
        }

        void IDisposable.Dispose() {
            //...
        }
    }
}
