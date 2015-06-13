using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody.Cecil
{
    public static class MethodDefinitionExtensions
    {

        public static MethodDefinition CopyToNewSibling(this MethodDefinition @this, string siblingName) 
        {
            var mNew = new MethodDefinition(
                                siblingName,
                                @this.Attributes,
                                @this.ReturnType
                                );

            @this.DeclaringType.Methods.Add(mNew);

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

            return mNew;
        }


    }
}
