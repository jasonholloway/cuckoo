using Cuckoo.Fody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CuckooConsumer
{

    //CuckooConsumer needs to install Cuckoo, and any other harmless mediating assemblies of its own.

    
    //There will be a shadow dependency graph for the compile time weaver assemblies.
    //Each weaving assembly will be a development dependency only.

    //Each library, Cuckoo included, will be bifurcated: back-end, front-end.
    //back-end files are copied into the relevant packages folder. Fody then finds these magically.

    //So a single package has two prongs: the unreferenced package files, and those copied to the binary folder.
    //The former are available to the weaving, the latter are not.

    //A consuming library must be structured the same: it will have background files copied to the package folder,
    //and a front-facing mediator module. The mediator module must reference the Cuckoo mediator module, but nothing else.
    //Though: all cuckoo and hatcher classes must be supplied by the front assembly.

    //The backing, package-based module, may contain little more than the subclassed ModuleWeaver itself, though would probably
    //contain the targeter. Cuckoos and hatchers would be in front module.


    //Cuckoo ideas: memoizer cache, logger, timer. Only the former is complicated enough to be worth it.
    
    //How about some coroutine-related mod?  


    public class ModuleWeaver : ModuleWeaverBase
    {        
        public override void Execute() {
            LogInfo("Cuckoo.ConsumingLibrary");
            AddTargeter<Targeter>();
            base.Execute();
        }
    }
}
