using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class Async : MarshalByRefObject {

        [BareCuckoo]
        public async void VoidAsyncMethod() {
            await Task.Run(() => Thread.Sleep(500));
        }



        public int IntAsyncMethodRunner() {
            return IntAsyncMethod().Result;
        }


        [BareCuckoo]
        async Task<int> IntAsyncMethod() {
            return await Task.Run(() => {
                                    Thread.Sleep(500);
                                    return 123;
                                });
        }

        //[BareCuckoo]
        //public async void OuterAsyncMethod() {
        //    int i = await IntAsyncMethod();
        //}


    }
}
