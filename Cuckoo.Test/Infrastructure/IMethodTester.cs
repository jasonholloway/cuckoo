using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    public interface IMethodTester
    {
        IClassMethodTester<TClass> WithClass<TClass>();
        IStaticMethodTester Static();
    }
    
    public interface IClassMethodTester<TClass>
    {
        void Run(Action<TClass> exFn);
        TResult Run<TResult>(Func<TClass, TResult> exFn);
        MethodInfo GetMethod(Func<MethodInfo, bool> fnSelect);
    }

    public interface IStaticMethodTester
    {
        TResult Run<TResult>(Expression<Func<TResult>> exFn);
    }

}
