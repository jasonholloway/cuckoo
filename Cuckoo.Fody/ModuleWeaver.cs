using System;
using System.Collections.Generic;
using System.Linq;

namespace Cuckoo.Fody
{
    public class ModuleWeaver : ModuleWeaverBase
    {                
        public override void Execute() {
            AddTargeters(ConfigReader.Read(base.Config));
            base.Execute();
        }
    }
    
}
