using Cuckoo.Common;
using Cuckoo.Gather;
using Cuckoo.Gather.Monikers;
using Cuckoo.Weave;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Cuckoo.Fody
{
    public abstract class ModuleWeaverBase
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogError { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public string AssemblyFilePath { get; set; }
        public string AddinDirectoryPath { get; set; }
        public string References { get; set; }
        public List<string> ReferenceCopyLocalPaths { get; set; }
        public XElement Config { get; set; }


        List<ITypeMoniker> _targeterMonikers = new List<ITypeMoniker>();
        
        protected void AddTargeter<T>() where T : IRoostTargeter, new() {
            AddTargeter(typeof(T));
        }

        protected void AddTargeter(Type type) {
            var g = new MonikerGenerator();
            AddTargeter(g.Type(type));
        }

        protected void AddTargeter(ITypeMoniker moniker) {
            _targeterMonikers.Add(moniker);
        }

        protected void AddTargeters(IEnumerable<ITypeMoniker> monikers) {
            _targeterMonikers.AddRange(monikers);
        }





        public virtual void Execute() {
            var logger = new Logger(LogInfo, LogError);
            
            logger.Info("Cuckoo: {0} targeters chosen...", _targeterMonikers.Count());
            
            var assemblyLocator = BuildAssemblyLocator();

            var gatherer = new Gatherer(
                                    AddinDirectoryPath,
                                    ModuleDefinition.Assembly.FullName,
                                    _targeterMonikers,
                                    assemblyLocator,
                                    logger);

            var roostSpecs = gatherer.Gather();

            logger.Info("Cuckoo: {0} roosts targeted...", roostSpecs.Count());

            if(roostSpecs.Any()) {
                var weaver = new Weaver(
                                    ModuleDefinition.Assembly,
                                    roostSpecs,
                                    logger);

                weaver.Weave();

                logger.Info("Cuckoo: All cuckoos nested!");
            }
        }


        AssemblyLocator BuildAssemblyLocator() {
            var paths = References.Split(';')
                            .Concat(new string[] { AssemblyFilePath });

            var d = paths.ToDictionary(
                            p => AssemblyName.GetAssemblyName(p).FullName,
                            p => p);

            return new AssemblyLocator(d);
        }

    }
}
