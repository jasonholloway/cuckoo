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
        ModuleDefinition _mod;
        MethodDefinition _method;
        ElementNameProvider _nameSource;
        Ref _ref;

        class Ref
        {
            public readonly TypeReference ICallUsurper_TypeRef;
            public readonly TypeReference CallSite_TypeRef;
            public readonly TypeReference CallArg_TypeRef;
            public readonly TypeReference ICall_TypeRef;
            public readonly TypeReference MethodInfo_TypeRef;

            public readonly MethodReference CallArg_mCtor;
            public readonly MethodReference CallArg_mGetIsPristine;
            public readonly MethodReference CallArg_mGetValue;
            public readonly MethodReference CallSite_mGetParams;
            public readonly MethodReference CallSite_mGetUsurpers;
            public readonly MethodReference CallSite_mCtor;
            public readonly MethodReference ICallUsurper_mInit;
            public readonly MethodReference ICallUsurper_mUsurp;
            public readonly MethodReference UsurpedAtt_mCtor;
            public readonly MethodReference MethodInfo_mGetMethodFromHandle;

            public Ref(ModuleDefinition module, MethodDefinition method) {
                ICallUsurper_TypeRef = module.ImportReference(typeof(ICallUsurper));
                CallSite_TypeRef = module.ImportReference(typeof(Cuckoo.Common.CallSite));
                CallArg_TypeRef = module.ImportReference(typeof(CallArg));
                ICall_TypeRef = module.ImportReference(typeof(ICall));
                MethodInfo_TypeRef = module.ImportReference(typeof(Refl.MethodInfo));

                CallArg_mCtor = module.ImportReference(
                                            CallArg_TypeRef.Resolve().GetConstructors().First());

                CallArg_mGetIsPristine = module.ImportReference(
                                            CallArg_TypeRef.GetMethod("get_IsPristine"));

                CallArg_mGetValue = module.ImportReference(
                                            CallArg_TypeRef.GetMethod("get_Value"));

                CallSite_mGetParams = module.ImportReference(
                                            CallSite_TypeRef.GetMethod("get_Parameters"));

                CallSite_mGetUsurpers = module.ImportReference(
                                            CallSite_TypeRef.GetMethod("get_Usurpers"));

                CallSite_mCtor = module.ImportReference(
                                            CallSite_TypeRef.Resolve().GetConstructors().First());

                ICallUsurper_mInit = module.ImportReference(
                                            typeof(ICallUsurper).GetMethod("Init"));

                ICallUsurper_mUsurp = module.ImportReference(
                                            typeof(ICallUsurper).GetMethod("Usurp"));

                UsurpedAtt_mCtor = module.ImportReference(
                                            typeof(UsurpedAttribute).GetConstructor(new[] { typeof(string) }));

                MethodInfo_mGetMethodFromHandle =
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


            }
        }



        public Weave(WeaveSpec spec, WeaveContext ctx) {
            _spec = spec;
            _ctx = ctx;
            _mod = _ctx.Module;
            _method = _spec.Method;
            _nameSource = new ElementNameProvider(_method.DeclaringType);
            _ref = new Ref(_ctx.Module, _spec.Method);
        }





        public void Apply() 
        {            
            var R = _ref;

            var tContainer = _method.DeclaringType;



            //********************************************************************************************
            //////////////////////////////////////////////////
            //Copy original method to usurped inner
            string usurpedName = _nameSource.GetElementName("USURPED", _method.Name); 

            var mUsurped = _method.CopyToNewSibling(usurpedName);
            
            mUsurped.Attributes |= MethodAttributes.Private;

            

            //////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            string callSiteName = _nameSource.GetElementName("CALLSITE", _method.Name);

            var fDec_CallSite = tContainer.AddField(
                                                R.CallSite_TypeRef, 
                                                callSiteName );

            fDec_CallSite.Attributes = FieldAttributes.Private
                                        | FieldAttributes.Static
                                        | FieldAttributes.InitOnly;

            tContainer.AppendToStaticCtor(
                (i, m) => {
                    var vMethodInfo = m.Body.AddVariable<Refl.MethodInfo>();
                    var vUsurper = m.Body.AddVariable<ICallUsurper>();
                    var vUsurpers = m.Body.AddVariable<ICallUsurper[]>();
                    
                    i.Emit(OpCodes.Ldtoken, _method);
                    i.Emit(OpCodes.Call, R.MethodInfo_mGetMethodFromHandle);
                    i.Emit(OpCodes.Stloc, vMethodInfo);

                    /////////////////////////////////////////////////////////////////////
                    //Load usurper instances into array ////////////////////////////////
                    i.Emit(OpCodes.Ldc_I4, _spec.CuckooAttributes.Length);
                    i.Emit(OpCodes.Newarr, R.ICallUsurper_TypeRef);
                    i.Emit(OpCodes.Stloc_S, vUsurpers);
                    
                    int iA = 0;

                    foreach(var att in _spec.CuckooAttributes) {
                        var tAtt = _mod.ImportReference(att.AttributeType);
                        
                        if(att.HasConstructorArguments) {
                            var mCtor = _mod.ImportReference(
                                                    tAtt.Resolve()
                                                        .GetConstructors()
                                                        .First(c => c.Parameters
                                                                        .Select(p => p.ParameterType)
                                                                        .SequenceEqual(
                                                                            att.ConstructorArguments
                                                                                .Select(a => a.Type)
                                                                            )
                                                        )
                                                    );

                            foreach(var ctorArg in att.ConstructorArguments) {
                                i.EmitConstant(ctorArg.Type, ctorArg.Value);
                            }

                            i.Emit(OpCodes.Newobj, mCtor);
                        }
                        else {
                            var mCtor = _mod.ImportReference(
                                                    tAtt.Resolve()
                                                        .GetConstructors()
                                                        .First(c => !c.HasParameters)
                                                    );

                            i.Emit(OpCodes.Newobj, mCtor);
                        }

                        i.Emit(OpCodes.Stloc, vUsurper);


                        if(att.HasFields) {
                            foreach(var namedCtorArg in att.Fields) {
                                var field = tAtt.Resolve().Fields
                                                    .First(f => f.Name == namedCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vUsurper);
                                i.EmitConstant(namedCtorArg.Argument.Type, namedCtorArg.Argument.Value);
                                i.Emit(OpCodes.Stfld, field);
                            }
                        }

                        if(att.HasProperties) {
                            foreach(var propCtorArg in att.Properties) {
                                var prop = tAtt.Resolve().Properties
                                                .Where(p => p.SetMethod != null)
                                                .First(p => p.Name == propCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vUsurper);
                                i.EmitConstant(propCtorArg.Argument.Type, propCtorArg.Argument.Value);
                                i.Emit(OpCodes.Call, prop.SetMethod);
                            }
                        }
                        
                        i.Emit(OpCodes.Ldloc_S, vUsurpers);
                        i.Emit(OpCodes.Ldc_I4, iA);
                        i.Emit(OpCodes.Ldloc_S, vUsurper);
                        i.Emit(OpCodes.Stelem_Ref);
                        
                        iA++;
                    }


                    ////////////////////////////////////////////////////////////////////
                    //Construct and store CallSite
                    i.Emit(OpCodes.Ldloc, vMethodInfo);
                    i.Emit(OpCodes.Ldloc, vUsurpers);
                    i.Emit(OpCodes.Newobj, R.CallSite_mCtor);
                    i.Emit(OpCodes.Stsfld, fDec_CallSite);


                    ////////////////////////////////////////////////////////////////////
                    //Init usurpers ///////////////////////////////////////////////////
                    for(int iU = 0; iU < _spec.CuckooAttributes.Length; iU++) {
                        i.Emit(OpCodes.Ldloc, vUsurpers);
                        i.Emit(OpCodes.Ldc_I4, iU);
                        i.Emit(OpCodes.Ldelem_Ref);

                        i.Emit(OpCodes.Ldloc, vMethodInfo);
                        i.Emit(OpCodes.Call, R.ICallUsurper_mInit);
                    }
                });
            


            /////////////////////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////////////////
            //Declare new ICall class /////////////////////////////////////////////////////////////////
            string callClassName = _nameSource.GetElementName("CALL", _method.Name);

            var tCall = new TypeDefinition(
                                tContainer.Namespace,
                                callClassName,
                                TypeAttributes.Class | TypeAttributes.NestedPrivate
                                );

            tContainer.NestedTypes.Add(tCall);

            tCall.BaseType = _mod.TypeSystem.Object;
            tCall.Interfaces.Add(R.ICall_TypeRef);

            var fCall_Instance = tCall.AddField<object>("_instance");
            var fCall_CallSite = tCall.AddField<Cuckoo.Common.CallSite>("_callSite");


            FieldDefinition fCall_Return = null;

            if(_method.ReturnsValue()) {
                fCall_Return = tCall.AddField(_method.ReturnType, "_return");
                fCall_Return.Attributes = FieldAttributes.Public;
            }


            var rfCall_ArgValue = _method.Parameters
                                            .Select(p => tCall.AddField(p.ParameterType, "_argValue_" + p.Name))
                                            .ToArray();

            var rCtorArgTypes = new[] { 
                                    _mod.ImportReference(typeof(object)), 
                                    _mod.ImportReference(typeof(Refl.MethodInfo)) 
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
                        R.ICall_TypeRef,
                        "get_Instance",
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_Instance);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        R.ICall_TypeRef,
                        "get_Method",
                        (i, m) => {
                            var mCallSite_GetMethod = _mod.ImportReference(
                                                                R.CallSite_TypeRef.Resolve().GetMethod("get_Method"));
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_CallSite);
                            i.Emit(OpCodes.Call, mCallSite_GetMethod);
                            i.Emit(OpCodes.Ret);
                        });


            var fCall_Args = tCall.AddField<CallArg[]>("_rArgs");

            tCall.OverrideMethod( //CallArgs need to be created lazily 
                        R.ICall_TypeRef,
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
                            i.Emit(OpCodes.Newarr, R.CallArg_TypeRef);
                            i.Emit(OpCodes.Stloc_S, vArgs);
                            
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fCall_CallSite);
                            i.Emit(OpCodes.Call, R.CallSite_mGetParams);
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

                                i.Emit(OpCodes.Newobj, R.CallArg_mCtor);

                                i.Emit(OpCodes.Stelem_Ref);
                            }

                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Stfld, fCall_Args);

                            i.Emit(OpCodes.Ldloc_S, vArgs);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        R.ICall_TypeRef,
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
                        R.ICall_TypeRef,
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
                        R.ICall_TypeRef,
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
                                i.Emit(OpCodes.Call, R.CallArg_mGetIsPristine);
                                i.Emit(OpCodes.Brtrue_S, lbSkipArgUpdate);

                                //update arg value here
                                i.Emit(OpCodes.Ldarg_0);

                                i.Emit(OpCodes.Ldloc_S, vArg);
                                i.Emit(OpCodes.Call, R.CallArg_mGetValue);
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
            var mUsurper = _method;
            mUsurper.Body = new MethodBody(mUsurper);

            mUsurper.Compose(
                (i, m) => {
                    var vCall = m.Body.AddVariable<ICall>();

                    i.Emit(OpCodes.Ldsfld, fDec_CallSite);

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


                    i.Emit(OpCodes.Ldsfld, fDec_CallSite);
                    i.Emit(OpCodes.Call, R.CallSite_mGetUsurpers);
                    i.Emit(OpCodes.Ldc_I4_0);
                    i.Emit(OpCodes.Ldelem_Ref); //!!!!!!!!!!!!!!!!!!!!!!!!!
                    
                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Call, R.ICallUsurper_mUsurp);

                    if(fCall_Return != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, fCall_Return);
                    }

                    i.Emit(OpCodes.Ret);
                });




            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Add a simple attribute to mark our usurpation ///////////////////////////////////////////////
            var atUsurped = new CustomAttribute(R.UsurpedAtt_mCtor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(_mod.TypeSystem.String, mUsurped.Name));

            mUsurper.CustomAttributes.Add(atUsurped);


            _ctx.Log("Mod applied to {0}!", mUsurper.FullName);
        }





        TypeDefinition CreateCallClass(MethodReference innerMethod) {
            var R = _ref;

            





            throw new NotImplementedException();
        }



    }
}
