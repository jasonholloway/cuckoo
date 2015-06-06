using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UsurpedAttribute : Attribute
    {
        public string InnerMethodName { get; private set; }

        public UsurpedAttribute(string innerMethodName) {
            InnerMethodName = innerMethodName;
        }
    }
}
