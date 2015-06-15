using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class Args
    {

        [ArgChangingCuckoo]
        public string MethodReturnsStrings(int a, string b, string c, float d, string e) {
            return string.Format("{0}! {1}! {2}!", b, c, e);
        }


        [BareCuckoo]
        public string MethodWithOutArg(int a, out int b, out string s) {
            b = 666;
            s = "Surprise!";
            return "hello";
        }


        [BareCuckoo]
        [ArgCuckoo("")]
        public string MethodWithOutArgAndManyCuckoos(int a, out int b, out string s) {
            b = 666;
            s = "Surprise!";
            return "hello";
        }



        //[BareCuckoo]
        //public string MethodWithRefArg(ref string s) {
        //    s = "dreariment";
        //    return "yup";
        //}




        //[OutArgChangingCuckoo]
        //public string MethodWithChangedOutArg(int a, out int b) {
        //    b = 44;
        //    return "hello";
        //}

        //[RefArgChangingCuckoo]
        //public string MethodWithChangedRefArg(ref string s) {
        //    s = "repetition";
        //    return "yup";
        //}




    }
}
