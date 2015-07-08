using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Specs;
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

namespace Cuckoo.Test
{
    [TestClass]
    public class MethodMonikerTests
    {
        [TestMethod]
        public void SimpleMethods() {
            TestMethodMonikers(new Expression<Action>[] {
                                        () => int.Parse("", null),
                                        () => new object().GetType()
                                        });
        }

        [TestMethod]
        public void GenericMethods() {
            throw new NotImplementedException();
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


        void TestMethodMonikers(IEnumerable<MethodBase> methods) {
            var refs = methods.Select(m => Method2MethodRefViaMoniker(m));

            var zipped = methods.Zip(refs, (m, r) => new { Method = m, MethodRef = r });

            foreach(var z in zipped) {
                var n = z.MethodRef.FullName;
                Assert.IsTrue(n == z.Method.Name);
            }
        }



        MethodReference Method2MethodRefViaMoniker(MethodBase method) 
        {
            var mod = ModuleDefinition.ReadModule(
                                        method.DeclaringType.Assembly.Location);

            var moniker = MethodMoniker.Derive(method);

            return mod.ImportMethodMoniker(moniker);
        }


    }
}
