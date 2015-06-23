using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericExample
{
    static class Class2
    {


        public static object CreateArray(int i) {
            var r = new object[] { 1, 2, 3, i, 5 };

            return r;
        }


        public static void Method() {
            /*
            object obj = null;


            var c = new Class1<int, int>();

            int i = c.Method1();

            i--;*/
        }

    }
}
