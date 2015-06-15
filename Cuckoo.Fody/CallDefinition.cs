using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class CallDefinition
    {
        public TypeDefinition tCall;
        public MethodDefinition mCtor;
        public FieldDefinition fReturn;

        public CallReference GetReferences(TypeReference[] genArgs) {
            //Should get references to each of the offered elements
            //...

            throw new NotImplementedException();
        }




        //expose array of args mapped to mOuter parameter here...

    }
}
