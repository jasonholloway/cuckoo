using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{
    public abstract class CtorClassBase
    {
        public int BaseValue { get; private set; }

        public CtorClassBase(int baseValue) {
            BaseValue = baseValue;
        }
    }

    public class CtorClass : CtorClassBase
    {
        public int DerivedValue { get; private set; }

        [BareCuckoo]
        [CtorArgChangingCuckoo]
        public CtorClass(int baseValue, int derivedValue)
            : base(baseValue * 100) {
            DerivedValue = derivedValue;
        }


        [CheckInstanceInPlaceCuckoo]
        public CtorClass(int a, int b, int c)
            : base(1) {
            DerivedValue = a;
        }


    }


}
