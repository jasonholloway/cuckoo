using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Fody.Cecil
{
    public static class TypeDefinitionExtensions
    {
        
        #region GetField

        public static FieldDefinition GetField(
            this TypeDefinition @this, 
            string fieldName) 
        {
            return @this.Fields.First(f => f.Name == fieldName);
        }

        public static FieldDefinition GetField(
            this TypeReference @this,
            string fieldName) 
        {
            return @this.Resolve().GetField(fieldName);
        }

        #endregion
        
        #region AddMethod

        public static MethodDefinition AddMethod(
            this TypeDefinition @this, 
            string name, 
            ParameterDefinition[] rParams, 
            TypeReference returnTypeRef, 
            Action<ILProcessor, ParameterDefinition[]> fnIL) 
        {
            var m = new MethodDefinition(name, MethodAttributes.Public, returnTypeRef);

            foreach(var param in rParams) {
                m.Parameters.Add(param);
            }

            @this.Methods.Add(m);

            m.Body.InitLocals = true;

            fnIL(m.Body.GetILProcessor(), rParams);

            return m;
        }

        public static MethodDefinition AddMethod(
            this TypeDefinition @this,
            string name,
            TypeReference[] rArgTypeRefs,
            TypeReference returnTypeRef,
            Action<ILProcessor, ParameterDefinition[]> fnIL) 
        {
            var rParams = rArgTypeRefs
                            .Select(r => new ParameterDefinition(r))
                            .ToArray();

            return @this.AddMethod(name, rParams, returnTypeRef, fnIL);
        }

        public static MethodDefinition AddMethod(
            this TypeDefinition @this,
            string name,
            Type[] rArgTypes,
            Type returnType,
            Action<ILProcessor, ParameterDefinition[]> fnIL) 
        {
            var rArgTypeRefs = rArgTypes
                                    .Select(t => @this.Module.Import(t))
                                    .ToArray();

            var returnTypeRef = @this.Module.Import(returnType);

            return @this.AddMethod(name, rArgTypeRefs, returnTypeRef, fnIL);
        }

        #endregion


        #region OverrideMethod

        public static MethodDefinition OverrideMethod(
            this TypeDefinition @this, 
            MethodReference baseMethodRef, 
            Action<ILProcessor, MethodDefinition> fnIL ) 
        {
            var module = @this.Module;

            baseMethodRef = module.Import(baseMethodRef);
            var baseMethod = baseMethodRef.Resolve();

            var m = new MethodDefinition(
                baseMethod.Name,
                baseMethod.Attributes ^ (MethodAttributes.Abstract | MethodAttributes.NewSlot),
                module.Import(baseMethod.ReturnType)
                );

            @this.Methods.Add(m);

            var rParams = baseMethod.Parameters
                                        .Select(p => new ParameterDefinition(
                                                            module.Import(p.ParameterType)
                                                            ))
                                        .ToArray();

            foreach(var param in rParams) {
                m.Parameters.Add(param);
            }

            m.Overrides.Add(baseMethodRef);

            m.Body.InitLocals = true;

            fnIL(m.Body.GetILProcessor(), m);

            return m;
        }

        public static MethodDefinition OverrideMethod(
            this TypeDefinition @this,
            string methodName,
            TypeReference declaringTypeRef,
            Action<ILProcessor, MethodDefinition> fnIL) 
        {
            var decType = declaringTypeRef.Resolve();

            var baseMethod = @this.Module.Import(
                                                decType.Methods.First(m => m.Name == methodName)
                                                );

            return @this.OverrideMethod(baseMethod, fnIL);
        }

        #endregion


        #region AddField

        public static FieldDefinition AddField(
            this TypeDefinition @this, 
            TypeReference typeRef, 
            string name,
            FieldAttributes atts = FieldAttributes.Private ) 
        {
            var f = new FieldDefinition(
                            name,
                            atts,
                            @this.Module.Import(typeRef)
                            );

            @this.Fields.Add(f);

            return f;
        }

        public static FieldDefinition AddField<T>(
            this TypeDefinition @this, 
            string name,
            FieldAttributes atts = FieldAttributes.Private) 
        {
            return @this.AddField(@this.Module.Import(typeof(T)), name, atts);
        }

        #endregion


        #region AddCtor

        public static MethodDefinition AddCtor(
            this TypeDefinition @this,
            IEnumerable<ParameterDefinition> paramDefs,
            Action<ILProcessor, MethodDefinition> fnIL ) 
        {
            var mCtor = new MethodDefinition(
                                ".ctor",
                                MethodAttributes.Public
                                    | MethodAttributes.HideBySig
                                    | MethodAttributes.SpecialName
                                    | MethodAttributes.RTSpecialName,
                                @this.Module.TypeSystem.Void
                                );

            foreach(var paramDef in paramDefs) {
                mCtor.Parameters.Add(paramDef);
            }

            mCtor.Body.InitLocals = true;

            @this.Methods.Add(mCtor);

            fnIL(mCtor.Body.GetILProcessor(), mCtor);

            return mCtor;

        }


        public static MethodDefinition AddCtor(
            this TypeDefinition @this,
            IEnumerable<TypeReference> argTypes,
            Action<ILProcessor, MethodDefinition> fnIL ) 
        {
            var paramDefs = argTypes
                            .Select(t => new ParameterDefinition(@this.Module.Import(t)));

            return @this.AddCtor(paramDefs, fnIL);
        }


        public static MethodDefinition AddCtor(
            this TypeDefinition @this, 
            Type[] rArgTypes, 
            Action<ILProcessor, MethodDefinition> fnIL ) 
        {
            return @this.AddCtor(
                            rArgTypes.Select(t => @this.Module.Import(t)).ToArray(), 
                            fnIL );
        }

        #endregion


        public static MethodReference GetMethod(this TypeReference @this, string name) {
            return @this.Module.Import(
                                    @this.Resolve().Methods.First(m => m.Name == name)
                                    );
        }



        #region AddVariable

        public static VariableDefinition AddVariable(this MethodBody @this, TypeReference typeRef) {
            var v = new VariableDefinition(@this.Method.Module.Import(typeRef));
            @this.Variables.Add(v);
            return v;
        }

        public static VariableDefinition AddVariable(this MethodBody @this, Type type) {
            var typeRef = @this.Method.Module.Import(type);
            return @this.AddVariable(typeRef);
        }

        public static VariableDefinition AddVariable<T>(this MethodBody @this) {
            return @this.AddVariable(typeof(T));
        }

        #endregion


        public static void Compose(this MethodDefinition @this, Action<ILProcessor, MethodDefinition> fnIL) {
            if(!@this.HasBody) {
                @this.Body = new MethodBody(@this);
            }

            @this.Body.InitLocals = true;

            var il = @this.Body.GetILProcessor();

            fnIL(il, @this);
        }





        public static void AppendToStaticCtor(this TypeDefinition @this, Action<ILProcessor, MethodDefinition> fnIL) 
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

            mCtorStatic.Body.InitLocals = true;

            var insReturn = mCtorStatic.Body.Instructions.Last();
            mCtorStatic.Body.Instructions.Remove(insReturn);

            var il = mCtorStatic.Body.GetILProcessor();
            fnIL(il, mCtorStatic);

            mCtorStatic.Body.Instructions.Add(insReturn);
        }




    }
}
