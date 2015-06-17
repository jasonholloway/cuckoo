using Cuckoo.Common;
using Cuckoo.Common.Attributes;
using Cuckoo.Fody;
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
            string newAssemblyPath = assemblyPath.Replace(".dll", "2.dll");
            
            var asmResolver = new DefaultAssemblyResolver();

            var readerParams = new ReaderParameters() {
                AssemblyResolver = asmResolver,
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate 
            };

            var module = ModuleDefinition
                                        .ReadModule(assemblyPath, readerParams);

            module.Assembly.Name = new AssemblyNameDefinition("Woven", new Version());
            module.Name = "Woven";

            var weaver = new ModuleWeaver() {
                                        ModuleDefinition = module,
                                        LogInfo = _ => { }
                                    };

            weaver.Execute();


            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true
            };

            module.Write(newAssemblyPath, writerParams);


            _assembly = Assembly.LoadFile(newAssemblyPath);

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

        /*
        protected Module UsurpedModule {
            get { return _assembly.Modules.First(); }
        }*/

        protected IEnumerable<MethodInfo> UsurpedMethods {
            get { return _usurpedMethods; }
        }

        /*
        protected MethodInfo GetUsurpedMethod(string name) {
            return _usurpedMethods.First(m => m.Name == name);
        }
        */

    }
}
