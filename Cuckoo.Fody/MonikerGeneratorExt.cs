using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cuckoo.Gather.Monikers;
using Mono.Cecil;

namespace Cuckoo.Fody
{

    public class MonikerGeneratorExt : MonikerGenerator
    {
        IAssemblyResolver _resolver;

        public MonikerGeneratorExt(IAssemblyResolver resolver) {
            _resolver = resolver;
        }

        public ITypeMoniker Type(string asmName) {

            //SHOULD BE TYPERESOLVER!!!!!!!

            //resolving types - we have the directory of... what?             




            throw new NotImplementedException();
        }
    }

}
