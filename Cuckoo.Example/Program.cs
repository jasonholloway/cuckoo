using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Example
{
    class Program
    {
        static void Main(string[] args) {

            string m = GetMessage();
            Console.WriteLine(m);

            var a = new AClass();
            
            a.Hello();

            Console.WriteLine("Cuckooed by custom targeter: {0} ... {1}", a.Hello(), a.HelloAgain());
        }

        [TestCuckoo]
        static string GetMessage() {
            return "Not cuckooed";
        }        
        
    }


    class AClass
    {

        public string Hello() {
            return "Not cuckooed";
        }

        public string HelloAgain() {
            return "Not cuckooed";
        }

    }

}
