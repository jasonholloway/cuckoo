using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Fody;
using Cuckoo.Common;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;

namespace Cuckoo.Test
{
    [TestClass]
    public class WeavingTest
    {
        Assembly _assembly;
        MethodInfo[] _usurpedMethods;

        [TestInitialize]
        public void Setup() {
            
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

            var moduleDefinition = ModuleDefinition
                                        .ReadModule(assemblyPath, readerParams);


            var weaver = new ModuleWeaver() {
                                        ModuleDefinition = moduleDefinition,
                                        LogInfo = _ => { }
                                    };

            weaver.Execute();


            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true
            };

            moduleDefinition.Write(newAssemblyPath, writerParams);


            _assembly = Assembly.LoadFile(newAssemblyPath);

            _usurpedMethods = _assembly.GetTypes()
                                            .SelectMany(t => t.GetMethods())
                                            .Where(m => m.GetCustomAttribute<UsurpedAttribute>() != null)
                                            .ToArray();
        }


        MethodInfo GetUsurpedMethod(string name) {
            return _usurpedMethods.First(m => m.Name == name);
        }



        [TestMethod]
        public void UsurpationsInPlaceAndCallable() {
            Assert.IsTrue(_usurpedMethods.Any(), "No usurpations!");

            foreach(var method in _usurpedMethods) {
                MethodTester.Test(method); 
            }
        }


        [TestMethod]
        public void CallSitesInPlace() {
            foreach(var method in _usurpedMethods) {
                var fCallSite = method.DeclaringType.GetField(
                                                        "<CALLSITE>_" + method.Name, 
                                                        BindingFlags.Static | BindingFlags.NonPublic);

                object value = fCallSite.GetValue(null);
                Assert.IsTrue(value is Cuckoo.Common.CallSite);

                var callSite = value as Cuckoo.Common.CallSite;
                Assert.AreEqual(callSite.Method, method);
                Assert.IsTrue(callSite.Usurper is CuckooAttribute);
            }
        }

        [TestMethod]
        public void CuckooReturnsValue() {
            var method = GetUsurpedMethod("MethodReturnsString");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Hello from down below!");
        }


        [TestMethod]
        public void CuckooChangesReturnValue() {
            var method = GetUsurpedMethod("MethodWithChangeableReturn");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "CHANGED!");
        }


        [TestMethod]
        public void MultipleCuckoosWorkTogether() {
            var method = GetUsurpedMethod("MethodReturnsInt");

            int result = (int)MethodTester.Test(method);

            Assert.IsTrue(result == 13 + 8 - 10);
        }


        [TestMethod]
        public void CuckooChangesArgs() {
            var method = GetUsurpedMethod("MethodReturnsStrings");

            string result = (string)MethodTester.Test(method);

            Assert.IsTrue(result == "Growl! Growl! Growl!");
        }






    }
}
