using Cuckoo.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Targeters;

namespace Cuckoo.Gather
{
    public class Gatherer
    {
        string _baseDir;
        string _targetAsmName;
        ITypeMoniker[] _targeterTypes;
        AssemblyLocator _locator;
        Logger _log;


        public Gatherer(
                    string baseDir, 
                    string targetAsmName, 
                    IEnumerable<ITypeMoniker> targeterTypes, 
                    AssemblyLocator locator, 
                    Logger logger) 
        {
            _baseDir = baseDir;
            _targetAsmName = targetAsmName;
            _targeterTypes = targeterTypes.ToArray();
            _locator = locator;
            _log = logger;
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

                agent.Init(_locator);

                return agent.GatherAllRoostSpecs(
                                            _targetAsmName, 
                                            _targeterTypes 
                                            );
            }
            finally {
                AppDomain.Unload(appDom);
            }

        }

    }
}
