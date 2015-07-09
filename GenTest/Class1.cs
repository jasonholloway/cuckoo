using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenTest
{
    public class GenClass<TVar>
    {
        public TVar GenMethod<TMVar>(TVar v) {
            return v;
        }

    }




    static class CallerClass
    {
        public static void CAllerMEthod() {
            var g = new GenClass<int>();
            g.GenMethod<float>(123);
        }
    }


}
