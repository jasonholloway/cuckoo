using Cuckoo.EffusiveTargeterExample;
using Cuckoo.Gather;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Cuckoo.Test
{
    [TestClass]
    public class GatheringTests
    {        

        [TestMethod]
        public void GatheredMethodsAreSifted() 
        {                
            var specs = GatherFromAssembly(typeof(EffusiveTargeter).Assembly);

            foreach(var spec in specs) {
                var m = typeof(EffusiveTargeter).Assembly
                            .Modules.First()
                                .ResolveMethod(spec.MethodSpec.Token);

                //This will always be the case, even if garbled, just here for info
                Assert.IsTrue(m.Module.Assembly == typeof(EffusiveTargeter).Assembly);                    

                Assert.IsTrue(m.Name == spec.MethodSpec.Name, 
                        "Returning garbled tokens: probably methods from outside target assembly!");
            }
        }
        


        IEnumerable<RoostSpec> GatherFromAssembly(Assembly asm) 
        {
            var appDom = AppDomain.CreateDomain(
                                    "GatheringTests",
                                    null,
                                    new AppDomainSetup() {
                                        ApplicationBase = Path.GetDirectoryName(
                                                                typeof(GatherAgent).Assembly.Location)
                                    });

            try {
                var agent = (GatherAgent)appDom.CreateInstanceAndUnwrap(
                                                    typeof(GatherAgent).Assembly.FullName,
                                                    typeof(GatherAgent).FullName
                                                    );

                var locator = new AssemblyLocator(new Dictionary<string, string>() { 
                                                            { asm.FullName, asm.Location } });

                agent.Init(locator);

                return agent.GatherAllRoostSpecs(asm.FullName);
            }
            finally {
                AppDomain.Unload(appDom);
            }
        }


    }
}
