using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    class GenericClass<T>
    {
        public T Hello() {
            return default(T);
        }
    }


    static class GenericClassCaller
    {
        public static void Test() {
            var c = new GenericClass<object>();
            c.Hello();
        }
    }
}
