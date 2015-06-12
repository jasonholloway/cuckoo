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
    using Refl = System.Reflection;

    internal class Weave
    {
        WeaveSpec _spec;
        WeaveContext _ctx;

        public Weave(WeaveSpec spec, WeaveContext ctx) {
            _spec = spec;
            _ctx = ctx;
        }

        public void Apply() 
        {
            var module = _ctx.Module;
            var method = _spec.Method;
            var nameProvider = new ElementNameProvider(_spec.Method.DeclaringType);

            var tICallUsurper = module.ImportReference(typeof(ICallUsurper));
            var tCallSite = module.ImportReference(typeof(Cuckoo.Common.CallSite));
            var tCallArg = module.ImportReference(typeof(CallArg));
            var tICall = module.ImportReference(typeof(ICall));
            var tMethodInfo = module.ImportReference(typeof(Refl.MethodInfo));
            var tDeclaring = method.DeclaringType;
            
            var mCallArg_Ctor = module.ImportReference(
                                        tCallArg.Resolve().GetConstructors().First());

            var mCallArg_GetIsPristine = module.ImportReference(
                                        tCallArg.GetMethod("get_IsPristine"));

            var mCallArg_GetValue = module.ImportReference(
                                        tCallArg.GetMethod("get_Value"));

            var mCallSite_GetParams = module.ImportReference(
                                        tCallSite.GetMethod("get_Parameters"));

            var mCallSite_Ctor = module.ImportReference(
                                        tCallSite.Resolve().GetConstructors().First());

            var mICallUsurper_Init = module.ImportReference(
                                        typeof(ICallUsurper).GetMethod("Init"));

            var mUsurpedAtt_Ctor = module.ImportReference(
                                        typeof(UsurpedAttribute).GetConstructor(new[] { typeof(string) }));


            //********************************************************************************************
            //////////////////////////////////////////////////
            //Copy original method to usurped inner
            string usurpedName = nameProvider.GetElementName("USURPED", method.Name); 

            var mUsurped = method.CopyToNewSibling(usurpedName);
            
            mUsurped.Attributes |= MethodAttributes.Private;

            

            //////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            string callSiteName = nameProvider.GetElementName("CALLSITE", method.Name);

            var fCallSite = tDeclaring.AddField(
                                        tCallSite, 
                                        callSiteName );

            fCallSite.Attributes = FieldAttributes.Private
                                    | FieldAttributes.Static
                                    | FieldAttributes.InitOnly;


            tDeclaring.AppendToStaticCtor(
                (i, m) => {
                    var vMethodInfo = m.Body.AddVariable<Refl.MethodInfo>();
                    var vUsurper = m.Body.AddVariable<ICallUsurper>();


                    var mMethodInfoResolve = 
                            module.ImportReference(typeof(Refl.MethodBase)
                                                        .GetMethod(
                                                            "GetMethodFromHandle", 
                                                            Refl.BindingFlags.Static 
                                                                | Refl.BindingFlags.Public,
                                                            null,
                                                            new[] { typeof(RuntimeMethodHandle) },
                                                            null
                                                            )
                                                        );

                    i.Emit(OpCodes.Ldtoken, method);
                    i.Emit(OpCodes.Call, mMethodInfoResolve);
                    i.Emit(OpCodes.Stloc, vMethodInfo);


                    /////////////////////////////////////////////////////////////////////
                    //Load instance of attribute fulfilling ICallUsurper interface

                    //just use first attribute for prototype - MORE TO DO!
                    var atCuckoo = _spec.CuckooAttributes.First();
                    var tCuckoo = module.ImportReference(atCuckoo.AttributeType);

                    //construct with args
                    if(atCuckoo.HasConstructorArguments) {
                        //find att ctor matching given args - and what of optional args?
                        //this should be encapsulated nicely into a method finder
                        var mCtor = module.ImportReference(
                                                tCuckoo.Resolve()
                                                    .GetConstructors()
                                                    .First(c => c.Parameters
                                                                    .Select(p => p.ParameterType)
                                                                    .SequenceEqual(
                                                                        atCuckoo.ConstructorArguments
                                                                                        .Select(a => a.Type)
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
                        var mCtor = module.ImportReference(
                                                tCuckoo.Resolve()
                                                    .GetConstructors()
                                                    .First(c => !c.HasParameters)
                                                );

                        i.Emit(OpCodes.Newobj, mCtor);
                    }

                    i.Emit(OpCodes.Stloc, vUsurper);


                    if(atCuckoo.HasFields) {
                        foreach(var namedArg in atCuckoo.Fields) {
                            var field = tCuckoo.Resolve().Fields
                                                     .First(f => f.Name == namedArg.Name);

                            i.Emit(OpCodes.Ldloc, vUsurper);
                            i.EmitConstant(namedArg.Argument.Type, namedArg.Argument.Value);
                            i.Emit(OpCodes.Stfld, field);
                        }
                    }

                    if(atCuckoo.HasProperties) {
                        foreach(var namedArg in atCuckoo.Properties) {
                            var prop = tCuckoo.Resolve().Properties
                                            .Where(p => p.SetMethod != null)
                                            .First(p => p.Name == namedArg.Name);

                            i.Emit(OpCodes.Ldloc, vUsurper);
                            i.EmitConstant(namedArg.Argument.Type, namedArg.Argument.Value);
                            i.Emit(OpCodes.Call, prop.SetMethod);
                        }
                    }


                    ////////////////////////////////////////////////////////////////////
                    //Construct and store CallSite
                    i.Emit(OpCodes.Ldloc, vMethodInfo);
                    i.Emit(OpCodes.Ldloc, vUsurper);
                    i.Emit(OpCodes.Newobj, mCallSite_Ctor);
                    i.Emit(OpCodes.Stsfld, fCallSite);


                    ////////////////////////////////////////////////////////////////////
                    //Init usurper ////////////////////////////////////////////////////
                    i.Emit(OpCodes.Ldloc, vUsurper);
                    i.Emit(OpCodes.Ldloc, vMethodInfo);
                    i.Emit(OpCodes.Call, mICallUsurper_Init);
                });




            /////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////
            //Declare new ICall class /////////////////////////////////////////////////////////////////
            string callClassName = nameProvider.GetElementName("CALL", method.Name);

            var tCall = new TypeDefinition(
                                tDeclaring.Namespace,
                                callClassName,
                                TypeAttributes.Class | TypeAttributes.NestedPrivate
                                );

            tDeclaring.NestedTypes.Add(tCall);

            tCall.BaseType = module.TypeSystem.Object;

            tCall.Interfaces.Add(tICall);

            var fCall_Instance = tCall.AddField<object>("_instance");
            var fCall_CallSite = tCall.AddField<Cuckoo.Common.CallSite>("_callSite");


            FieldDefinition fCall_Return = null;

            if(method.ReturnsValue()) {
                fCall_Return = tCall.AddField(method.ReturnType, "_return");
                fCall_Return.Attributes = FieldAttributes.Public;
            }


            var rfCall_ArgValue = method.Parameters
                                            .Select(p => tCall.AddField(p.ParameterType, "_argValue_" + p.Name))
                                            .ToArray();

            var rCtorArgTypes = new[] { 
                                    module.ImportReference(typeof(object)), 
                                    module.ImportReference(typeof(Refl.MethodInfo)) 
                                }
                                .Concat(rfCall_ArgValue.Select(f => f.FieldType))
                                .ToArray();

            var mCallCtor = tCall.AddCtor(
                                    rCtorArgTypes,
                                    (i, m) => {
                                        i.Emit(OpCodes.Ldarg_0);
                                        i.Emit(OpCodes.Ldarg_1);
                                        i.Emit(OpCodes.Stfld, fCall_CallSite);

                                        i.Emit(OpCodes.Ldarg_0);
                                        i.Emit(OpCodes.Ldarg_2);
                                        i.Emit(OpCodes.Stfld, fCall_Instance);

                                        //put args into special fields
                                        int iP = 2;
                                        var rParams = m.Parameters.ToArray();

                                        foreach(var fArgValue in rfCall_ArgValue) {
                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Ldarg_S, rParams[iP]);
                                            i.Emit(OpCodes.Stfld, fArgValue);
                                            iP++;
                                        }

                                        i.Emit(OpCodes.Ret);
                                    });

            tCall.OverrideMethod(
                        tICall,
                        "get_Instance",
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Instance);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "get_Method",
                        (i, m) => {
                            var mCallSite_GetMethod = module.ImportReference(
                                                                tCallSite.Resolve().GetMethod("get_Method"));
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_CallSite);
                            i.Emit(OpCodes.Call, mCallSite_GetMethod);
                            i.Emit(OpCodes.Ret);
                        });


            var fCall_Args = tCall.AddField<CallArg[]>("_rArgs");

            tCall.OverrideMethod( //CallArgs need to be created lazily 
                        tICall,
                        "get_Args",
                        (i, m) => {
                            //if _rArgs not null, return that
                            var vArgs = m.Body.AddVariable<CallArg[]>();
                            var vParams = m.Body.AddVariable<Refl.ParameterInfo[]>();
                            var lbCreateArgs = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Args);
                            i.Emit(OpCodes.Stloc_S, vArgs);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue_S, lbCreateArgs);

                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ret);

                            i.Append(lbCreateArgs);
                            i.Emit(OpCodes.Ldc_I4, rfCall_ArgValue.Length);
                            i.Emit(OpCodes.Newarr, tCallArg);
                            i.Emit(OpCodes.Stloc_S, vArgs);
                            
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_CallSite);
                            i.Emit(OpCodes.Call, mCallSite_GetParams);
                            i.Emit(OpCodes.Stloc_S, vParams);
                                                        
                            for(int iA = 0; iA < rfCall_ArgValue.Length; iA++) {
                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, iA);

                                //load parameter
                                i.Emit(OpCodes.Ldloc_S, vParams);
                                i.Emit(OpCodes.Ldc_I4, iA);
                                i.Emit(OpCodes.Ldelem_Ref);
                                                                
                                //load value & box
                                var fArgValue = rfCall_ArgValue[iA];
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fArgValue);
                                if(fArgValue.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fArgValue.FieldType);
                                }

                                i.Emit(OpCodes.Newobj, mCallArg_Ctor);

                                i.Emit(OpCodes.Stelem_Ref);
                            }

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Stfld, fCall_Args);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "get_ReturnValue",
                        (i, m) => {
                            if(fCall_Return != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fCall_Return);

                                if(fCall_Return.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fCall_Return.FieldType);
                                }
                            }
                            else {
                                i.Emit(OpCodes.Ldnull);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "set_ReturnValue",
                        (i, m) => {
                            if(fCall_Return != null) {
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldarg_1);

                                if(fCall_Return.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Unbox_Any, fCall_Return.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, fCall_Return);
                            }

                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "CallInner",
                        (i, m) => {                         
                            ///////////////////////////////////////////////////////////////
                            //Update arg fields if args not pristine
                            var vArgs = i.Body.AddVariable<CallArg[]>();
                            var vArg = i.Body.AddVariable<CallArg>();
                            var lbSkipAllArgUpdates = i.Create(OpCodes.Nop);

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Args);
                            i.Emit(OpCodes.Stloc_S, vArgs);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ldnull);
                            i.Emit(OpCodes.Ceq);
                            i.Emit(OpCodes.Brtrue, lbSkipAllArgUpdates);

                            int iA = 0;

                            foreach(var fArgValue in rfCall_ArgValue) {
                                var lbSkipArgUpdate = i.Create(OpCodes.Nop);

                                i.Emit(OpCodes.Ldloc_S, vArgs);
                                i.Emit(OpCodes.Ldc_I4, iA);
                                i.Emit(OpCodes.Ldelem_Ref);
                                i.Emit(OpCodes.Stloc_S, vArg);

                                i.Emit(OpCodes.Ldloc_S, vArg);
                                i.Emit(OpCodes.Call, mCallArg_GetIsPristine);
                                i.Emit(OpCodes.Brtrue_S, lbSkipArgUpdate);

                                //update arg value here
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldloc_S, vArg);
                                i.Emit(OpCodes.Call, mCallArg_GetValue);
                                if(fArgValue.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Unbox_Any, fArgValue.FieldType);
                                }

                                i.Emit(OpCodes.Stfld, fArgValue);

                                i.Append(lbSkipArgUpdate);

                                iA++;
                            }

                            i.Append(lbSkipAllArgUpdates);
                            
                            ////////////////////////////////////////////////////////////
                            //Load args onto stack from typed fields
                            i.Emit(OpCodes.Ldarg_0);

                            foreach(var fArgValue in rfCall_ArgValue) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fArgValue);
                            }

                            ///////////////////////////////////////////////////////////
                            //Call usurped and store returned value
                            i.Emit(OpCodes.Call, mUsurped);

                            if(fCall_Return != null) {
                                var vReturn = m.Body.AddVariable(fCall_Return.FieldType);
                                i.Emit(OpCodes.Stloc, vReturn);
                                
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, fCall_Return);
                            }

                            i.Emit(OpCodes.Ret);
                        });




            
            ///////////////////////////////////////////////////////////////////////////////////////////////
            //Write new body to cuckooed method //////////////////////////////////////////////////////////
            var mUsurper = method;
            mUsurper.Body = new MethodBody(mUsurper);

            mUsurper.Compose(
                (i, m) => {
                    var vCall = m.Body.AddVariable<ICall>();

                    i.Emit(OpCodes.Ldsfld, fCallSite);

                    if(m.HasThis) {
                        i.Emit(OpCodes.Ldarg_0);
                    }
                    else {
                        i.Emit(OpCodes.Ldnull);
                    }
                    
                    foreach(var param in m.Parameters) {
                        i.Emit(OpCodes.Ldarg_S, param);
                    }

                    i.Emit(OpCodes.Newobj, mCallCtor);
                    i.Emit(OpCodes.Stloc_S, vCall);


                    i.Emit(OpCodes.Ldsfld, fCallSite);
                    i.Emit(OpCodes.Call, tCallSite.GetMethod("get_Usurper"));

                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Call, tICallUsurper.GetMethod("Usurp"));

                    if(fCall_Return != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, fCall_Return);
                    }

                    i.Emit(OpCodes.Ret);
                });




            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Add a simple attribute to mark our usurpation ///////////////////////////////////////////////
            var atUsurped = new CustomAttribute(mUsurpedAtt_Ctor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(module.TypeSystem.String, mUsurped.Name));

            mUsurper.CustomAttributes.Add(atUsurped);


            _ctx.Log("Mod applied to {0}!", mUsurper.FullName);
        }

    }
}
