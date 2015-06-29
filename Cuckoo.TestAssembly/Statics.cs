using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.TestAssembly.Cuckoos;

namespace Cuckoo.TestAssembly
{
    public class StaticMethods
    {
        [DeductingCuckoo(10)]
        [AddingCuckoo(30)]
        public static int StaticMethod(int a, int b) {
            return a + b;
        }
    }

    public static class StaticClass
    {
        [DeductingCuckoo(10)]
        [AddingCuckoo(30)]
        public static int StaticMethodInStaticClass(int a) {
            return a;
        }


        [DeductingCuckoo(100)]
        [AddingCuckoo(500)]
        public static int ExtensionMethod(this StaticMethods @this, int b) {
            return b;
        }

    }

}
