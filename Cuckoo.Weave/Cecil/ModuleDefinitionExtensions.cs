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



        public static MethodReference ImportMethodMoniker(this ModuleDefinition @this, IMethodMoniker methodMoniker, IAssemblyResolver resolver = null) 
        {
            if(methodMoniker is GenMethodSpec) {
                var spec = (GenMethodSpec)methodMoniker;

                var baseRef = @this.ImportMethodMoniker(spec.BaseMethod, resolver);

                return (MethodReference)baseRef.MakeGenericInstanceMethod(
                                                            spec.GenArgs
                                                                    .Select(a => @this.ImportTypeMoniker(a, resolver))
                                                                    .ToArray()  
                                                            );
            }
            
            if(methodMoniker is MethodDef) {
                var def = (MethodDef)methodMoniker;

                var type = @this.ImportTypeMoniker(def.DeclaringType, resolver);

                return type.ReferenceMethod(
                                    m => m.MetadataToken.ToInt32() == def.MetadataToken
                                    );
            }                         
            
            throw new NotImplementedException();
        }



        public static TypeReference ImportTypeMoniker(this ModuleDefinition @this, ITypeMoniker typeMoniker, IAssemblyResolver resolver = null) 
        {
            resolver = resolver ?? @this.AssemblyResolver;

            if(typeMoniker is ArrayTypeSpec) {
                var arraySpec = (ArrayTypeSpec)typeMoniker;

                var elTypeRef = @this.ImportTypeMoniker(arraySpec.ElementType, resolver);

                return elTypeRef.MakeArrayType(arraySpec.Rank);
            }
            else if(typeMoniker is GenTypeSpec) {
                var genSpec = (GenTypeSpec)typeMoniker;

                var elTypeRef = @this.ImportTypeMoniker(genSpec.ElementType, resolver);

                return elTypeRef.MakeGenericInstanceType(
                                        genSpec.GenArgs
                                                    .Select(a => @this.ImportTypeMoniker(a, resolver))
                                                    .ToArray()
                                        );
            }

            TypeReference typeRef = null;

            if(typeMoniker is NestedTypeDef) {
                var nestedDef = (NestedTypeDef)typeMoniker;

                var decTypeRef = @this.ImportTypeMoniker(nestedDef.DeclaringType, resolver);

                typeRef = decTypeRef.Resolve().NestedTypes
                                                  .First(n => n.Name == nestedDef.Name);
            }
            else {
                var typeDef = (TypeDef)typeMoniker;

                var asm = resolver.Resolve(typeDef.AssemblyName);

                if(asm == null) {
                    throw new InvalidOperationException(string.Format(
                                                                "Unable to resolve assembly {0}",
                                                                typeDef.AssemblyName ));
                }

                typeRef = (TypeReference)asm.MainModule.GetType(
                                                    string.Format("{0}.{1}", typeDef.Namespace, typeDef.Name));
            }

            typeRef = @this.Import(typeRef);

            return typeRef;
        }




    }
}
