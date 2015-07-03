using Cuckoo;
using Cuckoo.Fody;
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
        static Assembly _assembly;
        static MethodInfo[] _usurpedMethods;

        static IMethodTester _tester;
        static WeaverSandbox _sandbox;

        static WeavingTestBase2() {
            string projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Cuckoo.TestAssembly\Cuckoo.TestAssembly.csproj"));
            string asmPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\Cuckoo.TestAssembly.dll");

            var weaver = new ModuleWeaver();
            
            _sandbox = new WeaverSandbox(asmPath, weaver);

            _sandbox.Init();

            _tester = new MethodTester2(_sandbox);
        }
        
        protected IMethodTester Tester {
            get { return _tester; }
        }
        
        void IDisposable.Dispose() {
            //...
        }
    }
}
