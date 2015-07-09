using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave.Cecil
{
    public static class GenericParameterExtensions
    {
        public static TypeReference ResolveArg(this GenericParameter @this) {
            if(@this.Owner is GenericInstanceType && @this.Type == GenericParameterType.Type) {
                return ((GenericInstanceType)@this.Owner).GenericArguments[@this.Position];
            }

            if(@this.Owner is GenericInstanceMethod && @this.Type == GenericParameterType.Method) {
                return ((GenericInstanceMethod)@this.Owner).GenericArguments[@this.Position];
            }

            throw new InvalidOperationException();
        }

    }
}
