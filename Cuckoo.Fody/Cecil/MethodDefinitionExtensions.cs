using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Fody.Cecil
{
    public static class MethodDefinitionExtensions
    {
        public static MethodDefinition CloneTo(this MethodDefinition @this, TypeDefinition declaringType, Action<MethodDefinition> fnModify) 
        {
            var mNew = new MethodDefinition(
                                @this.Name,
                                @this.Attributes,
                                @this.ReturnType
                                );

            declaringType.Methods.Add(mNew);

            foreach(var param in @this.Parameters) {
                mNew.Parameters.Add(param); //params probably need to be cloned, and updated if generic
            }

            foreach(var genParam in @this.GenericParameters) {
                var newGenParam = new GenericParameter(
                                                genParam.Name, 
                                                mNew);

                mNew.GenericParameters.Add(newGenParam);
            }

            mNew.CallingConvention = @this.CallingConvention;
            mNew.ExplicitThis = @this.ExplicitThis;
            mNew.ImplAttributes = @this.ImplAttributes;

            mNew.Body = new MethodBody(mNew) {
                Scope = @this.Body.Scope,
                InitLocals = @this.Body.InitLocals,
                LocalVarToken = @this.Body.LocalVarToken,
                MaxStackSize = @this.Body.MaxStackSize,
                //...
            };

            foreach(var v in @this.Body.Variables) {
                mNew.Body.Variables.Add(v);
            }

            foreach(var inst in @this.Body.Instructions) {
                mNew.Body.Instructions.Add(inst);
            }

            foreach(var eh in @this.Body.ExceptionHandlers) {
                mNew.Body.ExceptionHandlers.Add(eh);
            }

            fnModify(mNew);

            return mNew;
        }

        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition mod) {
            return mod.Types.SelectMany(t => t.GetAllTypes());
        }

    }
}
