using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Cuckoo.Gather.Monikers; 
using Mono.Cecil;

namespace Cuckoo.Fody
{
    public static class ConfigReader
    {
        public static IEnumerable<ITypeMoniker> Read(XElement el) {            
            if(el.Name.LocalName != "Cuckoo") {
                throw new InvalidOperationException();
            }

            var targeterSpecs = el.Elements("Targeter")
                                    .Select(t => new {
                                                    FullName = (string)t.Attribute("FullName"),
                                                    Assembly = (string)t.Attribute("Assembly")
                                                });

            return targeterSpecs
                        .Select(s => new SimpleTypeMoniker(s.FullName, s.Assembly));
        }

        [Serializable]
        class SimpleTypeMoniker : ITypeMoniker
        {
            public string AssemblyName { get; private set; }
            public string Name { get; private set; }
            public string FullName { get; private set; }
            public string AssemblyQualifiedName { get; private set; }

            public SimpleTypeMoniker(string fullName, string assemblyName) {
                FullName = fullName;
                AssemblyName = assemblyName;

                AssemblyQualifiedName = string.Format(
                                                @"{0}, {1}",
                                                FullName,
                                                AssemblyName
                                                );
            }
        }

    }
}
