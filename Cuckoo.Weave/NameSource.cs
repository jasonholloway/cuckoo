using Mono.Cecil;
using System.Collections.Generic;

namespace Cuckoo.Weave
{
    class NameSource
    {
        HashSet<string> _hash;

        public NameSource(TypeDefinition typeDef) {
            _hash = new HashSet<string>();

            foreach(var c in typeDef.NestedTypes) {
                _hash.Add(c.Name);
            }

            foreach(var m in typeDef.Methods) {
                _hash.Add(m.Name);
            }

            foreach(var f in typeDef.Fields) {
                _hash.Add(f.Name);
            }

            foreach(var p in typeDef.Properties) {
                _hash.Add(p.Name);
            }
        }

        public string GetElementName(string elementType, string elementName) {
            int number = 1;

            elementName = elementName.Replace(".", "<>");

            while(true) {
                string name = string.Format(
                                        "<{1}{2}>_{0}", 
                                        elementName, 
                                        elementType, 
                                        number == 1 ? "" : number.ToString() 
                                        );

                if(!_hash.Contains(name)) {
                    _hash.Add(name);
                    return name;
                }

                number++;
            }
        }

    }
}
