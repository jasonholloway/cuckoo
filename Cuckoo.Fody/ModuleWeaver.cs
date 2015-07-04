using Mono.Cecil;
using Cuckoo.Weave.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Cuckoo.Weave;

namespace Cuckoo.Fody
{
    public class ModuleWeaver
    {
        public string AssemblyFilePath { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }
        public Action<string> LogInfo { get; set; }

        public void Execute() {
            var weaver = new Weaver();

            weaver.Init(
                    ModuleDefinition,
                    AssemblyFilePath,
                    LogInfo
                    );

            weaver.Weave();
        }

    }
}
