using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Weave.Cecil
{
    using Refl = System.Reflection;

    public static class IlProcessorExtensions
    {
        public static void InsertBefore(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions) {
            foreach(var instruction in instructions)
                processor.InsertBefore(target, instruction);
        }

        public static void InsertAfter(this ILProcessor processor, Instruction target, IEnumerable<Instruction> instructions) {
            foreach(var instruction in instructions) {
                processor.InsertAfter(target, instruction);
                target = instruction;
            }
        }


        public static Instruction EmitConstant(this ILProcessor @this, TypeReference type, object value) {
            var mod = @this.Body.Method.Module;

            if(type.IsArray) {
                var r = (Array)value;
                var tEl = type.GetElementType();

                @this.Emit(OpCodes.Ldc_I4, r.Length);
                @this.Emit(OpCodes.Newarr, tEl);

                for(int i = 0; i < r.Length; i++) {
                    @this.Emit(OpCodes.Dup);
                    @this.Emit(OpCodes.Ldc_I4, i);

                    object v = r.GetValue(i);
                    var t = v != null
                                ? mod.Import(v.GetType())
                                : tEl;

                    if(v == null) {
                        @this.Emit(OpCodes.Ldnull);
                    }
                    else {
                        @this.EmitConstant(t, v);
                    }

                    switch(tEl.FullName) {                        
                        case "System.Int32":
                            @this.Emit(OpCodes.Stelem_I4);
                            break;

                        case "System.Int64":
                            @this.Emit(OpCodes.Stelem_I8);
                            break;

                        case "System.Single":
                            @this.Emit(OpCodes.Stelem_R4);
                            break;

                        case "System.Double":
                            @this.Emit(OpCodes.Stelem_R8);
                            break;

                        default:
                            if(tEl.IsValueType) {
                                throw new NotImplementedException("Emitting arrays of such element type not yet supported!");
                            }

                            if(t.IsValueType) {
                                @this.Emit(OpCodes.Box, t);
                            }

                            @this.Emit(OpCodes.Stelem_Ref);
                            break;
                    }
                }

                return @this.Body.Instructions.Last();
            }


            switch(type.FullName) {
                case "System.Boolean":
                    return @this.EmitEx(OpCodes.Ldc_I4, (bool)value ? 1 : 0); //??????

                case "System.String":
                    return @this.EmitEx(OpCodes.Ldstr, (string)value);

                case "System.Int32":
                    return @this.EmitEx(OpCodes.Ldc_I4, (int)value);

                case "System.UInt32":
                    return @this.EmitEx(OpCodes.Ldc_I4, (int)(uint)value);

                case "System.Int64":
                    return @this.EmitEx(OpCodes.Ldc_I8, (long)value);

                case "System.UInt64":
                    return @this.EmitEx(OpCodes.Ldc_I8, (long)(ulong)value);

                case "System.Byte":
                    return @this.EmitEx(OpCodes.Ldc_I4, (int)(byte)value);

                case "System.SByte":
                    return @this.EmitEx(OpCodes.Ldc_I4_S, (sbyte)value);

                case "System.Char":
                    return @this.EmitEx(OpCodes.Ldc_I4, (char)value);

                case "System.Single":
                    return @this.EmitEx(OpCodes.Ldc_R4, (float)value);

                case "System.Double":
                    return @this.EmitEx(OpCodes.Ldc_R8, (double)value);

                case "System.RuntimeType":                    
                    var t = @this.Body.Method.Module.Import((Type)value);
                    @this.EmitEx(OpCodes.Ldtoken, t);

                    var mType_mGetTypeFromHandle = @this.Body.Method.Module
                                                        .Import(typeof(Type).GetMethod(
                                                                        "GetTypeFromHandle", 
                                                                        Refl.BindingFlags.Public    
                                                                        | Refl.BindingFlags.Static));
                    
                    return @this.EmitEx(OpCodes.Call, mType_mGetTypeFromHandle);
                    
                default:
                    throw new InvalidOperationException("Can't emit constant of such a type (for now!)...");
            }
        }


        #region EmitEx

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode) {
            var ins = Instruction.Create(opcode);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, byte value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, double value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, float value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Instruction target) {
            var ins = Instruction.Create(opcode, target);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Instruction[] targets) {
            var ins = Instruction.Create(opcode, targets);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, int value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, long value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Mono.Cecil.CallSite site) {
            var ins = Instruction.Create(opcode, site);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Mono.Cecil.FieldReference field) {
            var ins = Instruction.Create(opcode, field);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Mono.Cecil.MethodReference method) {
            var ins = Instruction.Create(opcode, method);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Mono.Cecil.ParameterDefinition param) {
            var ins = Instruction.Create(opcode, param);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, Mono.Cecil.TypeReference type) {
            var ins = Instruction.Create(opcode, type);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, sbyte value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, string value) {
            var ins = Instruction.Create(opcode, value);
            @this.Append(ins);
            return ins;
        }

        public static Instruction EmitEx(this ILProcessor @this, OpCode opcode, VariableDefinition variable) {
            var ins = Instruction.Create(opcode, variable);
            @this.Append(ins);
            return ins;
        }

        #endregion


        public static void Replace(
                                this ILProcessor @this, 
                                Func<Instruction, bool> fnMatch, 
                                Action<ILProcessor, MethodBody> fnReplace ) 
        {

        }


    }
}
