using Cuckoo.Gather.Monikers;
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



        public static MethodReference ImportMethodMoniker(this ModuleDefinition @this, IMethodMoniker methodMoniker) 
        {
            if(methodMoniker is GenMethodSpec) {
                var spec = (GenMethodSpec)methodMoniker;

                var baseRef = @this.ImportMethodMoniker(spec.BaseMethod);

                return (MethodReference)baseRef.MakeGenericInstanceMethod(
                                                            spec.GenArgs
                                                                    .Select(a => @this.ImportTypeMoniker(a))
                                                                    .ToArray()  
                                                            );
            }
            
            if(methodMoniker is MethodDef) {
                var def = (MethodDef)methodMoniker;

                var type = @this.ImportTypeMoniker(def.DeclaringType);

                return type.ReferenceMethod(
                                    m => m.MetadataToken.ToInt32() == def.MetadataToken
                                    );
            }                         
            
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
