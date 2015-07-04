using Cuckoo.TestAssembly.Cuckoos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly
{

    public class CtorRunner : MarshalByRefObject
    {
        public CtorClass CtorWithBaseCalculation(int baseValue, int derivedValue) {
            return new CtorClass(baseValue, derivedValue);
        }

        public CtorClass Ctor(int a, int b, int c) {
            return new CtorClass(a, b, c);
        }

    }

    [Serializable]
    public abstract class CtorClassBase
    {
        public int BaseValue { get; private set; }

        public CtorClassBase(int baseValue) {
            BaseValue = baseValue;
        }
    }

    [Serializable]
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
