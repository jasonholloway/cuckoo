using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class TypeSpecArgs {

        //[BareCuckoo]
        //[ReturnCallArgsCuckoo]
        //public ICallArg[] ReturnGenericCallArgs<A, B>(A a, B b) {
        //    return null;
        //}

        [ReturnCallArgsCuckoo]
        [BareCuckoo]
        public ICallArg[] MethodWithArrayArgs(int[] ri, string[] rs, double[] rd) {
            return null;
        }

        [ReturnCallArgsCuckoo]
        [BareCuckoo]
        public ICallArg[] MethodWithNullableArgs(int? i, float? f, ulong? ul) {
            return null;
        }




    }
}
