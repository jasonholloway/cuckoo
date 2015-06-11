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

            var tICallUsurper = module.ImportReference(typeof(ICallUsurper));
            var tCallSite = module.ImportReference(typeof(Cuckoo.Common.CallSite));
            var tICall = module.ImportReference(typeof(ICall));
            var tMethodInfo = module.ImportReference(typeof(Refl.MethodInfo));
            var tDeclaring = method.DeclaringType;


            //********************************************************************************************
            //////////////////////////////////////////////////
            //Copy original method to usurped inner
            var mUsurped = method.CopyToNewSibling("<USURPED>" + method.Name);
            mUsurped.Attributes |= MethodAttributes.Private;


            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            var fCallSite = tDeclaring.AddField(
                                        tCallSite, 
                                        "<CALLSITE>" + method.Name );

            fCallSite.Attributes = FieldAttributes.Private
                                    | FieldAttributes.Static
                                    | FieldAttributes.InitOnly;


            tDeclaring.AppendToStaticCtor(
                (i, m) => {
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


                    var vUsurper = new VariableDefinition("usurper", tICallUsurper);
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
                    var mCallSiteCtor = module.ImportReference(
                                                    tCallSite.Resolve().GetConstructors().First()
                                                    );
            
                    i.Emit(OpCodes.Newobj, mCallSiteCtor);
                    i.Emit(OpCodes.Stsfld, fCallSite);
                });




            ////////////////////////////////////////////////////////////////////////////////////////////
            //Declare new ICall class /////////////////////////////////////////////////////////////////
            var tCall = new TypeDefinition(
                                tDeclaring.Namespace,
                                "<CALL>" + method.Name,
                                TypeAttributes.Class | TypeAttributes.NestedPrivate
                                );

            tDeclaring.NestedTypes.Add(tCall);

            tCall.BaseType = module.TypeSystem.Object;

            tCall.Interfaces.Add(tICall);

            var fInstance = tCall.AddField<object>("_instance");
            var fMethodInfo = tCall.AddField<Refl.MethodInfo>("_methodInfo");


            FieldDefinition fReturn = null;

            if(method.ReturnsValue()) {
                fReturn = tCall.AddField(method.ReturnType, "_return");
                fReturn.Attributes = FieldAttributes.Public;
            }


            var rfArgs = method.Parameters
                                .Select(p => tCall.AddField(p.ParameterType, "_arg" + p.Name))
                                .ToArray();

            var rCtorArgTypes = new[] { 
                                    module.ImportReference(typeof(object)), 
                                    module.ImportReference(typeof(Refl.MethodInfo)) 
                                }
                                .Concat(rfArgs.Select(f => f.FieldType))
                                .ToArray();

            var mCallCtor = tCall.AddCtor(
                                    rCtorArgTypes,
                                    (i, m) => {
                                        i.Emit(OpCodes.Ldarg_0);
                                        i.Emit(OpCodes.Ldarg_1);
                                        i.Emit(OpCodes.Stfld, fInstance);

                                        i.Emit(OpCodes.Ldarg_0);
                                        i.Emit(OpCodes.Ldarg_2);
                                        i.Emit(OpCodes.Stfld, fMethodInfo);

                                        //put args into special fields
                                        int iP = 2;
                                        var rParams = m.Parameters.ToArray();

                                        foreach(var fArg in rfArgs) {
                                            i.Emit(OpCodes.Ldarg_0);
                                            i.Emit(OpCodes.Ldarg_S, rParams[iP]);
                                            i.Emit(OpCodes.Stfld, fArg);
                                            iP++;
                                        }

                                        i.Emit(OpCodes.Ret);
                                    });

            tCall.OverrideMethod(
                        tICall,
                        "get_Instance",
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fInstance);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "get_Method",
                        (i, m) => {
                            i.Emit(OpCodes.Ldarg_0);
                            i.Emit(OpCodes.Ldfld, fMethodInfo);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "get_Args",
                        (i, m) => {
                            var vArr = m.Body.AddVariable<object[]>();

                            i.Emit(OpCodes.Ldc_I4, rfArgs.Length);
                            i.Emit(OpCodes.Newarr, module.TypeSystem.Object);
                            i.Emit(OpCodes.Stloc_S, vArr);
                            
                            for(int iA = 0; iA < rfArgs.Length; iA++) {
                                var fArg = rfArgs[iA];

                                i.Emit(OpCodes.Ldloc_S, vArr);

                                i.Emit(OpCodes.Ldc_I4, iA);

                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fArg);
                                if(fArg.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fArg.FieldType);
                                }

                                i.Emit(OpCodes.Stelem_Ref);
                            }

                            i.Emit(OpCodes.Ldloc_S, vArr);
                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "get_ReturnValue",
                        (i, m) => {
                            if(fReturn != null) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fReturn);

                                if(fReturn.FieldType.IsValueType) {
                                    i.Emit(OpCodes.Box, fReturn.FieldType);
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
                            if(fReturn != null) {
                                //unbox and cast
                                //...
                            }

                            i.Emit(OpCodes.Ret);
                        });

            tCall.OverrideMethod(
                        tICall,
                        "CallInner",
                        (i, m) => {                            
                            i.Emit(OpCodes.Ldarg_0);

                            foreach(var fArg in rfArgs) {
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldfld, fArg);
                            }

                            i.Emit(OpCodes.Call, mUsurped);

                            if(fReturn != null) {
                                var vReturn = m.Body.AddVariable(fReturn.FieldType);
                                i.Emit(OpCodes.Stloc, vReturn);
                                
                                i.Emit(OpCodes.Ldarg_0);
                                i.Emit(OpCodes.Ldloc, vReturn);
                                i.Emit(OpCodes.Stfld, fReturn);
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

                    if(m.HasThis) {
                        i.Emit(OpCodes.Ldarg_0);
                    }
                    else {
                        i.Emit(OpCodes.Ldnull);
                    }

                    i.Emit(OpCodes.Ldsfld, fCallSite);
                    i.Emit(OpCodes.Call, tCallSite.GetMethod("get_Method"));

                    foreach(var param in m.Parameters) {
                        i.Emit(OpCodes.Ldarg_S, param);
                    }

                    i.Emit(OpCodes.Newobj, mCallCtor);
                    i.Emit(OpCodes.Stloc_S, vCall);


                    i.Emit(OpCodes.Ldsfld, fCallSite);
                    i.Emit(OpCodes.Call, tCallSite.GetMethod("get_Usurper"));

                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Call, tICallUsurper.GetMethod("Usurp"));

                    if(fReturn != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, fReturn);
                    }

                    i.Emit(OpCodes.Ret);
                });




            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Add a simple attribute to mark our usurpation ///////////////////////////////////////////////
            var mUsurpedAttCtor = module.ImportReference( 
                                            typeof(UsurpedAttribute).GetConstructor(new[] { typeof(string) })
                                            );

            var atUsurped = new CustomAttribute(mUsurpedAttCtor);

            atUsurped.ConstructorArguments.Add(
                new CustomAttributeArgument(module.TypeSystem.String, mUsurped.Name)
                );

            mUsurper.CustomAttributes.Add(atUsurped);


            _ctx.Log("Mod applied to {0}!", mUsurper.FullName);
        }

    }
}
