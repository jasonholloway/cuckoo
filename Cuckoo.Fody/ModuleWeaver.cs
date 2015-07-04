using Mono.Cecil;
using Cuckoo.Weave.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Cuckoo.Weave;
using Cuckoo.Gather;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public string AssemblyFilePath { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }

        public void Execute() 
        {
            var gatherer = new Gatherer(AssemblyFilePath);

            var roostSpecs = gatherer.Gather();

            var weaver = new Weaver(
                                ModuleDefinition.Assembly,
                                roostSpecs,
                                LogInfo);

            weaver.Weave();
        }

    }
}
