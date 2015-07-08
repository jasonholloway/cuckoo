using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Specs;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cuckoo.Weave.Cecil
{
    public static class ModuleDefinitionExtensions
    {

        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition mod) {
            return mod.Types.SelectMany(t => t.GetAllTypes());
        }



        public static MethodReference ImportMethodReference(this ModuleDefinition @this, IMethodMoniker methodMoniker) 
        {
            throw new NotImplementedException();
        }



        public static TypeReference ImportTypeMoniker(this ModuleDefinition @this, ITypeMoniker typeMoniker) 
        {
            if(typeMoniker is ArrayTypeSpec) {
                var arraySpec = (ArrayTypeSpec)typeMoniker;

                var elTypeRef = @this.ImportTypeMoniker(arraySpec.ElementType);

                return elTypeRef.MakeArrayType(arraySpec.Rank);
            }
            else if(typeMoniker is GenTypeSpec) {
                var genSpec = (GenTypeSpec)typeMoniker;

                var elTypeRef = @this.ImportTypeMoniker(genSpec.ElementType);

                return elTypeRef.MakeGenericInstanceType(
                                        genSpec.GenArgs
                                                    .Select(a => @this.ImportTypeMoniker(a))
                                                    .ToArray()
                                        );
            }

            TypeReference typeRef = null;

            if(typeMoniker is NestedTypeDef) {
                var nestedDef = (NestedTypeDef)typeMoniker;

                var decTypeRef = @this.ImportTypeMoniker(nestedDef.DeclaringType);

                typeRef = decTypeRef.Resolve().NestedTypes
                                                  .First(n => n.Name == nestedDef.Name);
            }
            else {
                var typeDef = (TypeDef)typeMoniker;

                var asm = @this.AssemblyResolver.Resolve(typeDef.AssemblyName);

                typeRef = (TypeReference)asm.MainModule.GetType(
                                                    string.Format("{0}.{1}", typeDef.Namespace, typeDef.Name));
            }

            typeRef = @this.Import(typeRef);

            return typeRef;
        }




    }
}
