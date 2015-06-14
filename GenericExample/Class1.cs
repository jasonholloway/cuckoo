using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericExample
{
    public class Class1<A, B>
    {


        public class Nested<C>
        {
            public C Method1() {
                object obj = null;
                var ab = (Class1<A, B>)obj;

                return default(C);
            }
        }



    }
}
