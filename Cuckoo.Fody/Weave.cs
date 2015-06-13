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

        public Weave(WeaveSpec spec, Action<string> logger) 
        {
            _spec = spec;

            _ctx = new WeaveContext() {
                DeclaringType = spec.Method.DeclaringType,
                Module = spec.Method.Module,
                OuterMethod = spec.Method,
                NameSource = new NameSource(_spec.Method.DeclaringType),
                RefMap = new RefMap(_spec.Method.Module, spec.Method),
                Logger = logger
            };
        }

        
        public void Apply() 
        {            
            CopyOuterToInner(_ctx);
            
            CreateCallSite(_ctx);
            
            var tCall = new FinalCallClassWeaver(_ctx).Build();

            for(int iU = 1; iU < _spec.CuckooAttributes.Length; iU++) {
                tCall = new MediateCallClassWeaver(_ctx, tCall, iU).Build();
            }

            UsurpOuterMethod(_ctx, tCall);

            AddUsurpedAttributeToOuter(_ctx);

            _ctx.Logger(string.Format("Mod applied to {0}!", _ctx.OuterMethod.FullName));
        }



        void AddUsurpedAttributeToOuter(WeaveContext ctx) {
            var R = ctx.RefMap;
            var mod = ctx.Module;
            var mInner = ctx.InnerMethod;
            var mOuter = ctx.OuterMethod;

            var atUsurped = new CustomAttribute(R.UsurpedAtt_mCtor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(mod.TypeSystem.String, mInner.Name));

            mOuter.CustomAttributes.Add(atUsurped);
        }



        MethodDefinition CopyOuterToInner(WeaveContext ctx) {
            var mOuter = ctx.OuterMethod;
            var names = ctx.NameSource;

            string innerName = names.GetElementName("USURPED", mOuter.Name);

            var mInner = mOuter.CopyToNewSibling(innerName);

            mInner.Attributes = MethodAttributes.Private 
                                | MethodAttributes.HideBySig 
                                | MethodAttributes.SpecialName;

            _ctx.InnerMethod = mInner;

            return mInner;
        }


        FieldDefinition CreateCallSite(WeaveContext ctx) {
            var mod = ctx.Module;
            var mOuter = ctx.OuterMethod;
            var mInner = ctx.InnerMethod;
            var names = ctx.NameSource;
            var tContainer = ctx.DeclaringType;
            var R = ctx.RefMap;

            //////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            string callSiteName = names.GetElementName("CALLSITE", mOuter.Name);

            var f = tContainer.AddField(
                                R.CallSite_TypeRef,
                                callSiteName);

            f.Attributes = FieldAttributes.Private
                                        | FieldAttributes.Static
                                        | FieldAttributes.InitOnly;

            ctx.CallSiteField = f;

            tContainer.AppendToStaticCtor(
                (i, m) => {
                    var vMethodInfo = m.Body.AddVariable(R.MethodInfo_TypeRef);
                    var vUsurper = m.Body.AddVariable<ICallUsurper>();
                    var vUsurpers = m.Body.AddVariable<ICallUsurper[]>();

                    i.Emit(OpCodes.Ldtoken, mOuter);
                    i.Emit(OpCodes.Call, R.MethodInfo_mGetMethodFromHandle);
                    i.Emit(OpCodes.Castclass, R.MethodInfo_TypeRef);
                    i.Emit(OpCodes.Stloc, vMethodInfo);

                    /////////////////////////////////////////////////////////////////////
                    //Load usurper instances into array ////////////////////////////////
                    i.Emit(OpCodes.Ldc_I4, _spec.CuckooAttributes.Length);
                    i.Emit(OpCodes.Newarr, R.ICallUsurper_TypeRef);
                    i.Emit(OpCodes.Stloc_S, vUsurpers);

                    int iA = 0;

                    foreach(var att in _spec.CuckooAttributes) {
                        var tAtt = mod.ImportReference(att.AttributeType);

                        if(att.HasConstructorArguments) {
                            var mCtor = mod.ImportReference(
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
                            var mCtor = mod.ImportReference(
                                                    tAtt.Resolve()
                                                        .GetConstructors()
                                                        .First(c => !c.HasParameters)
                                                    );

                            i.Emit(OpCodes.Newobj, mCtor);
                        }

                        i.Emit(OpCodes.Stloc, vUsurper);


                        if(att.HasFields) {
                            foreach(var namedCtorArg in att.Fields) {
                                var field = tAtt.Resolve().GetField(namedCtorArg.Name);

                                i.Emit(OpCodes.Ldloc, vUsurper);
                                i.Emit(OpCodes.Castclass, tAtt);
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
                                i.Emit(OpCodes.Castclass, tAtt);
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
                    i.Emit(OpCodes.Stsfld, f);


                    ////////////////////////////////////////////////////////////////////
                    //Init usurpers ///////////////////////////////////////////////////
                    for(int iU = 0; iU < _spec.CuckooAttributes.Length; iU++) {
                        i.Emit(OpCodes.Ldloc, vUsurpers);
                        i.Emit(OpCodes.Ldc_I4, iU);
                        i.Emit(OpCodes.Ldelem_Ref);

                        i.Emit(OpCodes.Ldloc, vMethodInfo);
                        i.Emit(OpCodes.Callvirt, R.ICallUsurper_mInit);
                    }
                });

            return f;
        }



        MethodDefinition UsurpOuterMethod(WeaveContext ctx, TypeReference tTopCall) {
            var mOuter = ctx.OuterMethod;
            var R = ctx.RefMap;
            var fCallSite = ctx.CallSiteField;

            if(tTopCall.HasGenericParameters) {
                tTopCall = tTopCall.MakeGenericInstanceType(mOuter.GenericParameters.ToArray());
            }

            var TopCall_mCtor = tTopCall.ReferenceMethod(c => c.IsConstructor);
            var TopCall_fReturn = tTopCall.ReferenceField("_return");

            mOuter.Body = new MethodBody(mOuter);

            mOuter.Compose(
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

                    i.Emit(OpCodes.Newobj, TopCall_mCtor);
                    i.Emit(OpCodes.Stloc_S, vCall);

                    i.Emit(OpCodes.Ldsfld, fCallSite);
                    i.Emit(OpCodes.Call, R.CallSite_mGetUsurpers);
                    i.Emit(OpCodes.Ldc_I4_0);
                    i.Emit(OpCodes.Ldelem_Ref);

                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Callvirt, R.ICallUsurper_mUsurp);

                    if(TopCall_fReturn != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Castclass, tTopCall);
                        i.Emit(OpCodes.Ldfld, TopCall_fReturn);
                    }

                    i.Emit(OpCodes.Ret);
                });

            return mOuter;
        }




    }
}
