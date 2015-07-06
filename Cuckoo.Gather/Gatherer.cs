using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather
{
    public class Gatherer
    {
        string _baseDir;
        string _targetAsmPath;
        string[] _auxAsmPaths;

        public Gatherer(string baseDir, string targetAsmPath, IEnumerable<string> auxAsmPaths) {
            _baseDir = baseDir;
            _targetAsmPath = targetAsmPath;
            _auxAsmPaths = auxAsmPaths.ToArray();
        }

        public IEnumerable<RoostSpec> Gather() {
            var appDom = AppDomain.CreateDomain(
                                    "CuckooGathering",
                                    null,
                                    new AppDomainSetup() {
                                        ApplicationBase = _baseDir,
                                        ShadowCopyFiles = "true",
                                    });
            try {
                var agent = (GatherAgent)appDom.CreateInstanceAndUnwrap(
                                                                typeof(GatherAgent).Assembly.FullName,
                                                                typeof(GatherAgent).FullName);

                agent.LoadAssemblies(_auxAsmPaths);

                //var hostedAssemblies = appDom.GetAssemblies();
                
                return agent.GatherAllRoostSpecs(_targetAsmPath);
            }
            finally {
                AppDomain.Unload(appDom);
            }

        }

    }
}
