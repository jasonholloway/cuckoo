using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody.Cecil
{
    public static class TypeDefinitionExtensions
    {
        public static void AppendToStaticCtor(this TypeDefinition @this, Action<ILProcessor> fnIL) 
        {
            var mCtorStatic = @this.GetStaticConstructor();

            if(mCtorStatic == null) {
                mCtorStatic = new MethodDefinition(
                                        ".cctor",
                                        MethodAttributes.Private
                                            | MethodAttributes.Static
                                            | MethodAttributes.HideBySig
                                            | MethodAttributes.SpecialName
                                            | MethodAttributes.RTSpecialName,
                                        @this.Module.TypeSystem.Void);

                mCtorStatic.Body.GetILProcessor().Emit(OpCodes.Ret);

                @this.Methods.Add(mCtorStatic);
            }

            var insReturn = mCtorStatic.Body.Instructions.Last();
            mCtorStatic.Body.Instructions.Remove(insReturn);

            var il = mCtorStatic.Body.GetILProcessor();
            fnIL(il);

            mCtorStatic.Body.Instructions.Add(insReturn);
        }
        
    }
}
