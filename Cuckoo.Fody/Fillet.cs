using Cuckoo.Common;
using Cuckoo.Fody.Cecil;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class Fillet
    {
        FilletSpec _spec;
        FilletContext _ctx;

        public Fillet(FilletSpec spec, FilletContext ctx) {
            _spec = spec;
            _ctx = ctx;
        }

        public void Apply() 
        {
            //********************************************************************************************
            //////////////////////////////////////////////////
            //Copy original method to usurped inner
            var mUsurped = _spec.Method.CopyToSibling("<USURPED>" + _spec.Method.Name);
            mUsurped.Attributes |= MethodAttributes.Private;


            //***************************************************************************************************
            //////////////////////////////////////////////////////////////////////////
            //Create static CallSite ////////////////////////////////////////////////
            var decType = _spec.Method.DeclaringType;

            var tCallSite = _ctx.Module.ImportReference(
                                                _ctx.CommonModule.GetType(typeof(Cuckoo.Common.CallSite).FullName)
                                                );

            var fCallSite = new FieldDefinition(
                                    "<CALLSITE>" + _spec.Method.Name,
                                    FieldAttributes.Private 
                                        | FieldAttributes.Static 
                                        | FieldAttributes.InitOnly,
                                    tCallSite
                                    );

            decType.Fields.Add(fCallSite);

            var tMethodInfo = _ctx.Module.ImportReference(typeof(System.Reflection.MethodInfo));

            decType.AppendToStaticCtor(i => {

                /////////////////////////////////////////////////////////////////////
                //Load methodinfo of target method
                var mMethodInfoResolve = 
                        _ctx.Module.ImportReference(typeof(System.Reflection.MethodBase)
                                                            .GetMethod(
                                                                "GetMethodFromHandle", 
                                                                System.Reflection.BindingFlags.Static 
                                                                    | System.Reflection.BindingFlags.Public,
                                                                null,
                                                                new[] { typeof(RuntimeMethodHandle) },
                                                                null
                                                                )
                                                            );

                i.Emit(OpCodes.Ldtoken, _spec.Method);
                i.Emit(OpCodes.Call, mMethodInfoResolve);



                /////////////////////////////////////////////////////////////////////
                //Load instance of attribute fulfilling ICallUsurper interface

                //just use first attribute for prototype
                var atCuckoo = _spec.CuckooAttributes.First();
                var tCuckoo = _ctx.Module.ImportReference(atCuckoo.AttributeType);

                //construct with args
                if(atCuckoo.HasConstructorArguments) {
                    //find att ctor matching given args - and what of optional args?
                    //this should be encapsulated nicely into a method finder
                    var mCtor = _ctx.Module.ImportReference(
                                                tCuckoo.Resolve()
                                                    .GetConstructors()
                                                    .First(m => m.Parameters
                                                                    .Select(p => p.ParameterType)
                                                                    .SequenceEqual(
                                                                        atCuckoo.ConstructorArguments.Select(a => a.Type)
                                                                        )
                                                    )
                                                );

                    //load args to stack
                    foreach(var arg in atCuckoo.ConstructorArguments) {
                        i.EmitConstant(arg.Type, arg.Value);
                    }

                    i.Emit(OpCodes.Newobj, mCtor);
                }
                else {
                    var mCtor = _ctx.Module.ImportReference(
                                                tCuckoo.Resolve()
                                                    .GetConstructors()
                                                    .First(m => !m.HasParameters)
                                                );

                    i.Emit(OpCodes.Newobj, mCtor);
                }



                var tUsurper = _ctx.Module.ImportReference(
                                            _ctx.CommonModule.GetType(typeof(ICallUsurper).FullName)
                                            );

                var vUsurper = new VariableDefinition("usurper", tUsurper);
                i.Body.Variables.Add(vUsurper);


                if(atCuckoo.HasFields) {
                    i.Emit(OpCodes.Stloc, vUsurper);

                    foreach(var namedArg in atCuckoo.Fields) {
                        var field = tCuckoo.Resolve().Fields
                                                 .First(f => f.Name == namedArg.Name);

                        i.Emit(OpCodes.Ldloc, vUsurper);
                        i.EmitConstant(namedArg.Argument.Type, namedArg.Argument.Value);
                        i.Emit(OpCodes.Stfld, field);
                    }

                    i.Emit(OpCodes.Ldloc, vUsurper);
                }

                if(atCuckoo.HasProperties) {
                    i.Emit(OpCodes.Stloc, vUsurper);

                    foreach(var namedArg in atCuckoo.Properties) {
                        var prop = tCuckoo.Resolve().Properties
                                        .Where(p => p.SetMethod != null)
                                        .First(p => p.Name == namedArg.Name);

                        i.Emit(OpCodes.Ldloc, vUsurper);
                        i.EmitConstant(namedArg.Argument.Type, namedArg.Argument.Value);
                        i.Emit(OpCodes.Call, prop.SetMethod);
                    }

                    i.Emit(OpCodes.Ldloc, vUsurper);
                }


                ////////////////////////////////////////////////////////////////////
                //Construct and store CallSite
                var mCallSiteCtor = _ctx.Module.ImportReference(
                                                    tCallSite.Resolve().GetConstructors().First()
                                                    );
            
                i.Emit(OpCodes.Newobj, mCallSiteCtor);
                i.Emit(OpCodes.Stsfld, fCallSite);
            });



            //********************************************************************************************
            //////////////////////////////////////////////////
            //Create new ICall class

            //ctor taking same arguments as mUsurped
            //populating set of correspondent private fields

            //CallInner overriden to push arguments from fields onto stack
            //and to call mUsurped


            //////////////////////////////////////////////////
            //Write new body to cuckooed method
            var mCuckoo = _spec.Method;
            mCuckoo.Body = new MethodBody(mCuckoo);

            //Construct call instance with method args
            //and pass to usurper
            //...

            //if necessary, push call.returnvalue onto stack for return
            //...

            //END!





            //


            var il = mCuckoo.Body.GetILProcessor();

            if(mCuckoo.HasThis) {
                il.Emit(OpCodes.Ldarg_S, mCuckoo.Body.ThisParameter);
            }

            foreach(var param in mCuckoo.Parameters) {
                il.Emit(OpCodes.Ldarg_S, param);
            }

            il.Emit(OpCodes.Call, mUsurped);
            
            il.Emit(OpCodes.Ret);


            ///////////////////////////////////////////////////////////
            //Attempt optimization (though doesn't seem to do much...
            mCuckoo.Body.OptimizeMacros();


            //////////////////////////////////////////////////////////
            //Add a simple attribute to mark our usurpation /////////
            var tAtt = _ctx.CommonModule
                            .GetType(typeof(UsurpedAttribute).FullName);

            var mAttCtor = _ctx.Module
                                .ImportReference(tAtt.GetConstructors().First());

            var att = new CustomAttribute(mAttCtor);

            att.ConstructorArguments.Add(
                new CustomAttributeArgument(_ctx.Module.TypeSystem.String, mUsurped.Name)
                );

            mCuckoo.CustomAttributes.Add(att);


            _ctx.Log("Mod applied to {0}!", mCuckoo.FullName);
        }

    }
}
