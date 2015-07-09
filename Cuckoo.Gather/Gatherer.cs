using Cuckoo.Common;
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
        string _targetAsmName;
        AssemblyLocator _locator;
        Logger _log;

        public Gatherer(string baseDir, string targetAsmName, AssemblyLocator locator, Logger logger) {
            _baseDir = baseDir;
            _targetAsmName = targetAsmName;
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

                var specs = agent.GatherAllRoostSpecs(_targetAsmName);

                return specs;
            }
            finally {
                AppDomain.Unload(appDom);
            }

        }

    }
}
