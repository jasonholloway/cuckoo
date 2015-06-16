using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericExample
{
    internal class ByRefs
    {

        public void ByRefMethod(int a, out int b, out string c) {
            b = 99;
            c = "BLAH";
        }



    }
}
