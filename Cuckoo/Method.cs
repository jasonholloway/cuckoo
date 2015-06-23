using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo
{
    public class Method
    {
        public string Name { get; private set; }
        public Type DeclaringType { get; private set; }

        public bool IsConstructor { get; private set; }

        public bool IsGeneric { get; private set; }
        public Type[] GenericArgTypes { get; private set; }

        public bool ReturnsValue { get; private set; }
        public Type ReturnType { get; private set; }


        public RuntimeMethodHandle RuntimeMethodHandle { get; private set; }


        public ConstructorInfo GetConstructorInfo() {
            return (ConstructorInfo)MethodBase.GetMethodFromHandle(RuntimeMethodHandle);
        }

        public MethodInfo GetMethodInfo() {
            return (MethodInfo)MethodBase.GetMethodFromHandle(RuntimeMethodHandle);
        } 
    }
}
