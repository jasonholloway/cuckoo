using Cuckoo.Gather.Monikers;
using Cuckoo.Test.Infrastructure;
using Cuckoo.Weave;
using Cuckoo.Weave.Cecil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AssertExLib;
using System.Reflection.Emit;
using Mono.Cecil.Cil;

namespace Cuckoo.Test
{
    [TestClass]
    public class MethodMonikerTests
    {
        [TestMethod]
        public void SimpleMethods() {
            TestMethodMonikers(new Expression<Action>[] {
                                        () => int.Parse("", null),
                                        () => new object().GetType(),
                                        () => new ArrayTypeSpec(null, 0).GetAssemblyQualifiedName(),
                                        () => "blah".EndsWith("", StringComparison.CurrentCultureIgnoreCase)
                                        });
        }







        class AClass
        {
            public void AMethod<T, T2>(T t, T2 t2) { }
        }

        class AnotherClass<TClass>
        {
            public void AMethod<T, T2>(T t, T2 t2, TClass tc) { }
        }




        [TestMethod]
        public void GenericMethods() {
            TestMethodMonikers(new Expression<Action>[] {
                                        () => new List<int>().OfType<byte>(),
                                        () => new AnotherClass<sbyte>().AMethod(1, 1F, -13),
                                        () => new AClass().AMethod(1D, 13F),
                                        });
        }

        [TestMethod]
        public void Constructors() {
            throw new NotImplementedException();
        }




        void TestMethodMonikers(Expression<Action>[] exps) {
            TestMethodMonikers(exps.Select(
                                        e => {
                                            if(e.Body is MethodCallExpression) {
                                                return (MethodBase)((MethodCallExpression)e.Body).Method;
                                            }

                                            if(e.Body is NewExpression) {
                                                return (MethodBase)((NewExpression)e.Body).Constructor;
                                            }

                                            throw new ArgumentException();
                                        }));
        }


        void TestMethodMonikers(IEnumerable<MethodBase> methods) 
        {
            var monikers = new MonikerGenerator();

            var refs = methods.Select(m => Method2MethodRefViaMoniker(m, monikers));

            var zipped = methods.Zip(refs, (m, r) => new { Method = m, MethodRef = r });

            foreach(var z in zipped) {
                var mRef = z.MethodRef;
                var mBase = z.Method;

                Assert.IsTrue(mRef.DeclaringType.GetAssemblyQualifiedName() == mBase.DeclaringType.AssemblyQualifiedName);
                Assert.IsTrue(mRef.Name == mBase.Name);

                Assert.IsTrue(mRef.Parameters.Select(p => p.ParameterType.GetAssemblyQualifiedName())
                                .SequenceEqual(mBase.GetParameters().Select(p => p.ParameterType.AssemblyQualifiedName)));

                Assert.IsTrue(mRef.IsGenericInstance == (mBase.IsGenericMethod && !mBase.IsGenericMethodDefinition));

                if(mRef.IsGenericInstance) {
                    Assert.IsTrue(((GenericInstanceMethod)mRef).GenericArguments.Select(a => a.GetAssemblyQualifiedName())
                                        .SequenceEqual(mBase.GetGenericArguments().Select(a => a.AssemblyQualifiedName)));
                }
            }
        }



        MethodReference Method2MethodRefViaMoniker(MethodBase method, MonikerGenerator monikers) 
        {
            var mod = ModuleDefinition.ReadModule(
                                        method.DeclaringType.Assembly.Location);

            var moniker = monikers.Method(method);

            return mod.ImportMethodMoniker(moniker);
        }


    }
}
