using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Gather
{
    public class AssemblyLocator : MarshalByRefObject
    {
        IDictionary<string, string> _dNames2Paths;

        public AssemblyLocator(IDictionary<string, string> dNames2Paths) {
            _dNames2Paths = dNames2Paths;
        }

        public string LocateAssembly(string fullName) {
            return _dNames2Paths[fullName];
        }

    }
}
