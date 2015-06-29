using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody.Cecil
{
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
            //int typeID = type.MetadataToken.ToInt32();

            switch(type.FullName) {
                case "System.Boolean": //This could surely be done by type-token id
                    return @this.EmitEx(OpCodes.Ldc_I4, (bool)value ? 1 : 0); //??????

                case "System.String":
                    return @this.EmitEx(OpCodes.Ldstr, (string)value);

                case "System.Int32":
                    return @this.EmitEx(OpCodes.Ldc_I4, (int)value);

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
