using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{


    static class ExampleClassCaller
    {
        public static void Caller() {
            var o = new ExampleClass<int>();


            o.SimpleMethod(123);

            //o.BoxOrNotBox<string>("GAH!");
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
