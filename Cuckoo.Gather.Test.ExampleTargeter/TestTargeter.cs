using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Gather.Test.ExampleTargeter
{
    public class Targeter1000 : BaseTargeter
    {
        public Targeter1000() : base(1000) { }
    }

    public class Targeter99 : BaseTargeter
    {
        public Targeter99() : base(99) { }
    }

    class Targeter13 : BaseTargeter
    {
        public Targeter13() : base(13) { }
    }



    public abstract class BaseTargeter : IRoostTargeter
    {
        int _count;

        protected BaseTargeter(int resultCount) {
            _count = resultCount;
        }

        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) 
        {
            var method = typeof(AClass).GetConstructor(Type.EmptyTypes);

            return Enumerable.Range(1, _count)
                                .Select(i => new RoostTarget(method, typeof(AClass)));
        }
    }


    public class AClass : ICuckooHatcher
    {
        public IEnumerable<ICuckoo> HatchCuckoos(IRoost roost) {
            throw new NotImplementedException();
        }
    }

}
