using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave.Cecil
{
    public static class TypeReferenceExtensions
    {
        public static bool ImplementsInterface<TInterface>(this TypeReference @this) {
            return @this.ImplementsInterface(typeof(TInterface).FullName);
        }

        public static bool ImplementsInterface(this TypeReference @this, string interfaceName) {
            var typeDef = @this.Resolve();

            return (typeDef.HasInterfaces
                            && typeDef.Interfaces.Any(t => t.FullName == interfaceName))
                    || (typeDef.BaseType != null
                            && typeDef.BaseType.ImplementsInterface(interfaceName));

        }




        public static string GetAssemblyQualifiedName(this TypeReference @this) {
            var sb = new StringBuilder();

            BuildClassName(@this, sb);

            if(@this.IsGenericInstance) {
                sb.Append("[");

                bool isSucceeder = false;

                foreach(var genArg in ((GenericInstanceType)@this).GenericArguments) {
                    if(isSucceeder) {
                        sb.Append(",");
                    }

                    sb.Append("[");
                    sb.Append(genArg.GetAssemblyQualifiedName());
                    sb.Append("]");
                    isSucceeder = true;
                }

                sb.Append("]");
            }

            sb.Append(", ");
            sb.Append(@this.IsGenericParameter 
                            ? ((GenericParameter)@this).Owner.Module.Assembly.FullName
                            : @this.Resolve().Module.Assembly.FullName);

            return sb.ToString();
        }



        static void BuildClassName(TypeReference typeRef, StringBuilder sb) {
            if(typeRef.IsNested) {
                BuildClassName(typeRef.DeclaringType, sb);
                sb.Append("+");
            }
            else {
                sb.Append(typeRef.Namespace);
                sb.Append(".");
            }

            sb.Append(typeRef.Name);
        }



    }
}
