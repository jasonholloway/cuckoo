using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class TypeSpecArgs : MarshalByRefObject
    {

        //[BareCuckoo]
        //[ReturnCallArgsCuckoo]
        //public ICallArg[] ReturnGenericCallArgs<A, B>(A a, B b) {
        //    return null;
        //}

        [ReturnCallArgTypesCuckoo]
        [BareCuckoo]
        public string[] MethodWithArrayArgs(int[] ri, string[] rs, double[] rd) {
            return null;
        }

        [ReturnCallArgTypesCuckoo]
        [BareCuckoo]
        public string[] MethodWithNullableArgs(int? i, float? f, ulong? ul) {
            return null;
        }




    }
}
