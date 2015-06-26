using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo
{
    public class ParamSpec
    {
        public string Name { get; private set; }

        public Type Type { get; private set; }
        public bool IsByRef { get; private set; }

        public MethodSpec Method { get; private set; }

        public ParameterInfo GetParameterInfo() {
            throw new NotImplementedException();
        }
    }
}
