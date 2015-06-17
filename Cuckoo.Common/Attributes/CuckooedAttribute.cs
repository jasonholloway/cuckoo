using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CuckooedAttribute : Attribute
    {
        public string InnerMethodName { get; private set; }

        public CuckooedAttribute(string innerMethodName) {
            InnerMethodName = innerMethodName;
        }
    }
}
