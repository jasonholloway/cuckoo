using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sequences;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    public class MethodTester2 : IMethodTester
    {
        WeaverSandbox _sandbox;

        public MethodTester2(WeaverSandbox sandbox) {
            _sandbox = sandbox;
        }

        public IClassMethodTester<TClass> WithClass<TClass>() {
            return new ClassMethodTester<TClass>(_sandbox);
        }


        public IStaticMethodTester Static() {
            return new StaticMethodTester(_sandbox);
        }



        class StaticMethodTester
            : IStaticMethodTester
        {
            WeaverSandbox _sandbox;

            public StaticMethodTester(WeaverSandbox sandbox) {
                _sandbox = sandbox;
            }

            public TResult Run<TResult>(Expression<Func<TResult>> exFn) {
                throw new NotImplementedException();
            }
        }


        class ClassMethodTester<TClass>
            : IClassMethodTester<TClass>
        {
            WeaverSandbox _sandbox;

            public ClassMethodTester(WeaverSandbox sandbox) {
                _sandbox = sandbox;
            }

            public void Run(Action<TClass> exFn) {
                _sandbox.Run(appDom => {
                    TClass instance = (TClass)appDom.CreateInstanceAndUnwrap(
                                                            typeof(TClass).Assembly.FullName,
                                                            typeof(TClass).FullName
                                                            );

                    exFn(instance);
                });
            }


            public TResult Run<TResult>(Func<TClass, TResult> exFn) {
                TResult result = default(TResult);

                _sandbox.Run(appDom => {
                    TClass instance = (TClass)appDom.CreateInstanceAndUnwrap(
                                                            typeof(TClass).Assembly.FullName,
                                                            typeof(TClass).FullName
                                                            );
                    result = exFn(instance);
                });
                
                return result;
            }


            public MethodInfo GetMethod(Func<MethodInfo, bool> fnSelect) {
                throw new NotImplementedException();

                //var baseType = _mapper.Map(typeof(TClass));
                //var method = baseType.GetMethods().Single(fnSelect);
                //return method;
            }

        }



    }
}
