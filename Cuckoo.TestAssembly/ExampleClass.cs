using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Cuckoo.TestAssembly
{


    static class ExampleClassCaller
    {
        public static void Caller() {
            var o = new ExampleClass<int>();


            o.SimpleMethod(123);

            //o.BoxOrNotBox<string>("GAH!");




            MethodBase methodBase = null;
            MethodInfo methodInfo = (MethodInfo)methodBase;



            ConstructorInfo ctorInfo;



        }
    }


    class ExampleClass<T>
    {

        public T SimpleMethod(int i) {
            return default(T);
        }

        public B BoxOrNotBox<B>(object obj) {
            return (B)obj;
        }

    }
}
