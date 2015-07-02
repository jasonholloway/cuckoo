using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public class Virtuals
    {
        [DeductingCuckoo(100)]
        public virtual int VirtualMethod(int a) {
            return 98765;
        }
    }


    public class VirtualsDerived : Virtuals
    {
        public override int VirtualMethod(int a) {
            return 456;
        }
    }


    public abstract class AbstractBaseClass
    {
        [DeductingCuckoo(50)]
        public abstract int AbstractMethod(int a);

        [DeductingCuckoo(100)]
        public int ConcreteMethod(int a) {
            return a;
        }


    }

    public class DerivedFromAbstractClass : AbstractBaseClass
    {
        public override int AbstractMethod(int a) {
            return 77;
        }
    }

}
