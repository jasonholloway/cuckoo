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
                                        () => new MethodReference(null, null).ReturnsValue(),
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


        class YetAnotherClass
        {
            public YetAnotherClass() {

            }

            public YetAnotherClass(params int[] r) {

            }

            public YetAnotherClass(int i, int e) {

            }
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
            TestMethodMonikers(new Expression<Action>[] {
                                        () => new AClass(),
                                        () => new AnotherClass<byte>(),
                                        () => new string('e', 23),
                                        () => new List<int>(new[] { 1, 2, 3 }),
                                        () => new YetAnotherClass(),
                                        () => new YetAnotherClass(1),
                                        () => new YetAnotherClass(1, 2)
                                        });
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

                var zippedParams = mRef.ResolveParamTypes().Zip(
                                                mBase.GetParameters()
                                                            .Select(p => p.ParameterType),
                                                (t1, t2) => new {
                                                    CecilParamType = t1,
                                                    ReflParamType = t2
                                                });
                                
                foreach(var zp in zippedParams) {
                    if(zp.CecilParamType is GenericParameter) {
                        throw new NotImplementedException();
                    }
                    else if(zp.CecilParamType.IsGenericInstance) {
                        Assert.IsTrue(zp.CecilParamType.GetElementType().GetAssemblyQualifiedName()
                                                        == zp.ReflParamType.GetGenericTypeDefinition().AssemblyQualifiedName);

                        //should really check resolved args here too
                        //...
                    }
                    else {
                        Assert.IsTrue(zp.CecilParamType.GetAssemblyQualifiedName()
                                                        == zp.ReflParamType.AssemblyQualifiedName);
                    }
                }

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
