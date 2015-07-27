using Cuckoo.Gather.Monikers;
using Cuckoo.Test.Infrastructure;
using Cuckoo.Weave;
using Cuckoo.Weave.Cecil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test
{
    [TestClass]
    public class TypeMonikerTests
    {


        [TestMethod]
        public void PrimitiveTypes() {
            TestTypeMonikers(new[] {
                            typeof(int),
                            typeof(bool),
                            typeof(string)
                        });
        }
        
        [TestMethod]
        public void SimpleTypes() {
            TestTypeMonikers(new[] {
                            typeof(TypeMonikerTests),
                            typeof(Weaver),
                            typeof(CuckooTestContext)
                        });
        }



        class NestedClass {
            public class DoublyNestedClass { }
        }
        
        [TestMethod]
        public void NestedTypes() {
            TestTypeMonikers(new[] {
                            typeof(NestedClass),
                            typeof(NestedClass.DoublyNestedClass)
                        });
        }
        
        
        [TestMethod]
        public void ArrayTypes() {
            TestTypeMonikers(new[] {
                            typeof(TypeMonikerTests[]),
                            typeof(int[])
                        });
        }


        [TestMethod]
        public void MultiArrayTypes() {
            TestTypeMonikers(new[] {
                            typeof(TypeMonikerTests[][]),
                            typeof(int[,,][][,])
                        });
        }
                        

        [TestMethod]
        public void GenericTypes() {
            TestTypeMonikers(new[] {
                            typeof(List<int>),
                            typeof(Dictionary<string, ITypeMoniker>),
                            typeof(Stack<List<Queue<float>>>)
                        });

        }




        void TestTypeMonikers(Type[] types) 
        {
            var monikers = new MonikerGenerator();

            var typeRefs = types.Select(t => Type2TypeRefViaMoniker(t, monikers));

            var zipped = types.Zip(typeRefs, (t, r) => new { Type = t, TypeRef = r });

            foreach(var z in zipped) {
                var n = z.TypeRef.GetAssemblyQualifiedName();
                Assert.IsTrue(n == z.Type.AssemblyQualifiedName);
            }
        }



        TypeReference Type2TypeRefViaMoniker(Type type, MonikerGenerator monikers) {
            var mod = ModuleDefinition.ReadModule(
                                        type.Assembly.Location);

            var moniker = monikers.Type(type);

            return mod.ImportTypeMoniker(moniker);
        }


    }
}
