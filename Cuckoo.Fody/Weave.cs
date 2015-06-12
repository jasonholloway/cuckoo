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
        BuildContext _buildContext;
        Action<string> _logger;

        public Weave(WeaveSpec spec, Action<string> logger) {
            _buildContext = new BuildContext() {
                DeclaringType = spec.Method.DeclaringType,
                Module = spec.Method.Module,
                OuterMethod = spec.Method,
                NameSource = new ElementNameSource(_spec.Method.DeclaringType)
            };

            _buildContext.Ref = new Ref(_buildContext.Module, spec.Method);

            _spec = spec;
            _logger = logger;
        }


        public void Apply() 
        {
            var R = _buildContext.Ref;
            var tContainer = _buildContext.DeclaringType;
            var mOuter = _buildContext.OuterMethod;
            var nameSource = _buildContext.NameSource;
            var mod = _buildContext.Module;

            //********************************************************************************************
            //////////////////////////////////////////////////
            //Copy original method to usurped inner
            string usurpedName = nameSource.GetElementName("USURPED", mOuter.Name); 

            var mInner = mOuter.CopyToNewSibling(usurpedName);
     
            mInner.Attributes |= MethodAttributes.Private;

            _buildContext.InnerMethod = mInner;


            //////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////
            //Create static CallSite ///////////////////////////////////////////////////////////////////
            string callSiteName = nameSource.GetElementName("CALLSITE", mOuter.Name);

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
                    
                    i.Emit(OpCodes.Ldtoken, mOuter);
                    i.Emit(OpCodes.Call, R.MethodInfo_mGetMethodFromHandle);
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
            //Declare new ICall classes ///////////////////////////////////////////////////////////////

            var callClassBuilder = new FinalCallClassBuilder(_buildContext);

            var rtCalls = new[] { 
                callClassBuilder.Build()
            };


            ///////////////////////////////////////////////////////////////////////////////////////////////
            //Write new body to cuckooed method //////////////////////////////////////////////////////////
            mOuter.Body = new MethodBody(mOuter);

            var TopCall_mCtor = rtCalls.First().GetConstructors().First();
            var TopCall_fReturn = rtCalls.First().Fields.FirstOrDefault(f => f.Name == "_return");
            
            mOuter.Compose(
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

                    i.Emit(OpCodes.Newobj, TopCall_mCtor);
                    i.Emit(OpCodes.Stloc_S, vCall);

                    i.Emit(OpCodes.Ldsfld, fDec_CallSite);
                    i.Emit(OpCodes.Call, R.CallSite_mGetUsurpers);
                    i.Emit(OpCodes.Ldc_I4_0);
                    i.Emit(OpCodes.Ldelem_Ref); //!!!!!!!!!!!!!!!!!!!!!!!!!

                    i.Emit(OpCodes.Ldloc, vCall);

                    i.Emit(OpCodes.Call, R.ICallUsurper_mUsurp);

                    if(TopCall_fReturn != null) {
                        i.Emit(OpCodes.Ldloc, vCall);
                        i.Emit(OpCodes.Ldfld, TopCall_fReturn);
                    }

                    i.Emit(OpCodes.Ret);
                });


            ////////////////////////////////////////////////////////////////////////////////////////////////
            //Add a simple attribute to mark our usurpation ///////////////////////////////////////////////
            var atUsurped = new CustomAttribute(R.UsurpedAtt_mCtor);
            atUsurped.ConstructorArguments.Add(
                            new CustomAttributeArgument(mod.TypeSystem.String, mInner.Name));

            mOuter.CustomAttributes.Add(atUsurped);


            _logger(string.Format("Mod applied to {0}!", mOuter.FullName));
        }








    }
}
