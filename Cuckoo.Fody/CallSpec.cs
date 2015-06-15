using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class CallSpec
    {
        public TypeReference Type { get; private set; }
        public CallArgSpec[] Args { get; private set; }
        public FieldReference ReturnField { get; private set; }

        public CallSpec(TypeReference type, CallArgSpec[] args, FieldReference returnField) {
            Type = type;
            Args = args;
            ReturnField = returnField;
        }
    }


    internal class CallArgSpec
    {
        public FieldReference Field { get; private set; }
        public ParameterReference Param { get; private set; }

        public CallArgSpec(FieldReference field, ParameterReference param) {
            Field = field;
            Param = param;
        }
    }

}
