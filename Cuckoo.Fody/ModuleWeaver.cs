using Cuckoo.Common;
using Cuckoo.Gather;
using Cuckoo.Weave;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Targeters;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogError { get; set; }
        public IAssemblyResolver AssemblyResolver { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public string AssemblyFilePath { get; set; }
        public string AddinDirectoryPath { get; set; }
        public string References { get; set; }
        public List<string> ReferenceCopyLocalPaths { get; set; }
        

        public void Execute() 
        {
            var logger = new Logger(LogInfo, LogError);

            var locator = BuildAssemblyLocator();



            //targeters to be resolved from passed parameters!
            //AssemblyLocator needs to be able to service whatever's specified, too.


            var monikers = new MonikerGenerator();

            var targeterTypes = new[] { 
                                    monikers.Type(typeof(AttributeTargeter)),
                                    monikers.Type(typeof(CascadeTargeter))
                                };


            var gatherer = new Gatherer(
                                    AddinDirectoryPath,
                                    ModuleDefinition.Assembly.FullName,
                                    targeterTypes,
                                    locator,
                                    logger );

            var roostSpecs = gatherer.Gather();

            logger.Info("Cuckoo.Fody: {0} roosts identified...", roostSpecs.Count());

            var weaver = new Weaver(
                                ModuleDefinition.Assembly,
                                roostSpecs,
                                logger );

            weaver.Weave();

            logger.Info("Cuckoo.Fody: All cuckoos nested!");
        }


        AssemblyLocator BuildAssemblyLocator() {
            var paths = References.Split(';')
                            .Concat(new string[] { AssemblyFilePath });

            var d = paths.ToDictionary(
                            p => AssemblyName.GetAssemblyName(p).FullName, 
                            p => p );

            return new AssemblyLocator(d);
        }

    }
}
