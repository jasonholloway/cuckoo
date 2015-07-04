using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather
{
    public class Gatherer
    {
        string _asmPath;

        public Gatherer(string assemblyPath) {
            _asmPath = assemblyPath;
        }

        public IEnumerable<RoostSpec> Gather() {
            var childAppDomain = AppDomain.CreateDomain(
                                            "CuckooGathering",
                                            null,
                                            new AppDomainSetup() {
                                                ShadowCopyFiles = "true"
                                            });
            try {
                var targetAssembly = childAppDomain.Load(AssemblyName.GetAssemblyName(_asmPath));

                var agent = (GatherAgent)childAppDomain.CreateInstanceFromAndUnwrap(
                                                                typeof(GatherAgent).Assembly.Location,
                                                                typeof(GatherAgent).FullName);

                return agent.GatherAllRoostSpecs(targetAssembly.FullName);
            }
            finally {
                AppDomain.Unload(childAppDomain);
            }

        }

    }
}
