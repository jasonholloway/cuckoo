using Cuckoo.EffusiveTargeterExample;
using Cuckoo.Gather;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssertExLib;
using Cuckoo.Weave;
using Mono.Cecil;
using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Targeters;

namespace Cuckoo.Test
{
    [TestClass]
    public class GatheringTests
    {
        [TestMethod]
        public void GatheringBadMethodsThrowExceptions() {
            AssertEx.Throws<CuckooGatherException>(() => {
                GatherFromAssembly(typeof(EffusiveTargeter).Assembly);
            });
        }






        //[TestMethod]
        //public void GatheredMethodTokensNotGarbled() {
        //    var specs = GatherFromAssembly(typeof());

        //    foreach(var spec in specs) {
        //        var m = typeof(EffusiveTargeter).Assembly
        //                    .Modules.First()
        //                        .ResolveMethod(spec.MethodSpec.Token);

        //        //This will always be the case, even if garbled, just here for info
        //        Assert.IsTrue(m.Module.Assembly == typeof(EffusiveTargeter).Assembly);

        //        Assert.IsTrue(m.Name == spec.MethodSpec.Name,
        //                "Returning garbled tokens: probably methods from outside target assembly!");
        //    }
        //}
        


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


                var monikers = new MonikerGenerator();

                var targeterTypes = new[] { 
                                        monikers.Type(typeof(AttributeTargeter)),
                                        monikers.Type(typeof(CascadeTargeter))
                                    };

                return agent.GatherAllRoostSpecs(asm.FullName, targeterTypes.ToArray());
            }
            finally {
                AppDomain.Unload(appDom);
            }
        }


    }
}
