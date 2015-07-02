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
    public abstract class WeavingTestBase
    {        
        static Assembly _assembly;
        static MethodInfo[] _usurpedMethods;
        static MethodTester _tester;

        static WeavingTestBase() {
            string projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Cuckoo.TestAssembly\Cuckoo.TestAssembly.csproj"));
            string assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\Cuckoo.TestAssembly.dll");

            _assembly = WeaverRunner.Reweave(assemblyPath);

            _usurpedMethods = _assembly.GetTypes()
                                            .Select(t => {
                                                if(t.ContainsGenericParameters) {
                                                    var rtGenTypes = Sequence.Fill(typeof(int), t.GetGenericArguments().Length).ToArray();
                                                    t = t.MakeGenericType(rtGenTypes);
                                                }

                                                return t;
                                            })
                                            .SelectMany(t => t.GetMethods())
                                            .Where(m => m.GetCustomAttribute<CuckooedAttribute>() != null)
                                            .ToArray();

            _tester = new MethodTester(_assembly.Modules.First());
        }
        
        protected MethodTester Tester {
            get { return _tester; }
        }
        
        protected IEnumerable<MethodInfo> UsurpedMethods {
            get { return _usurpedMethods; }
        }

    }
}
