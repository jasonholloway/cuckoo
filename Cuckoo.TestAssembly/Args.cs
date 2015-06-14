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

        
        public string MethodWithOutArg(int a, out int b) {
            b = 44;
            return "hello";
        }


        public string MethodWithRefArg(ref string s) {
            s = "dreariment";
            return "yup";
        }




    }
}
