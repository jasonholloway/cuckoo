using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody.Cecil
{
    public static class MethodReferenceExtensions
    {

        public static GenericInstanceMethod MakeGenericInstanceMethod(this MethodReference @this, params TypeReference[] args) {
            var mInst = new GenericInstanceMethod(@this);

            foreach(var arg in args) {
                mInst.GenericArguments.Add(arg);
            }

            return mInst;
        }


    }
}
