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
    public abstract class WeaveTestBase : IDisposable
    {
        protected IMethodTester Tester { get; private set; }

        public WeaveTestBase() {
            this.Tester = new MethodTester2(CuckooTestContext.Sandbox);
        }

        void IDisposable.Dispose() {
            //...
        }
    }
}
