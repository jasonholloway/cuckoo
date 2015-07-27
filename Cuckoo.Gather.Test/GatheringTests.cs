using Cuckoo.EffusiveTargeterExample;
using Cuckoo.Gather;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssertExLib;
//using Cuckoo.Weave;
//using Mono.Cecil;
using Cuckoo.Gather.Monikers;
using Cuckoo.Gather.Targeters;
using Cuckoo.Gather.Test.ExampleTargeter;

namespace Cuckoo.Gather.Test
{
    [TestClass]
    public class GatheringTests
    {
        [TestMethod]
        public void GatheringBadMethodsThrowExceptions() {
            AssertEx.Throws<CuckooGatherException>(() => {
                GatherFromAssembly(
                    typeof(EffusiveTargeter).Assembly,
                    new[] { typeof(AttributeTargeter), typeof(CascadeTargeter) }
                    );
            });
        }
        

        [TestMethod]
        public void CorrectTargeterFoundInAssembly() 
        {
            var monikers = new MonikerGenerator();

            var specs = GatherFromAssembly(
                            typeof(Targeter99).Assembly,
                            new[] { typeof(Targeter99) }
                            );

            Assert.AreEqual(99, specs.Count());
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
        


        IEnumerable<RoostSpec> GatherFromAssembly(Assembly asm, Type[] targeterTypes) 
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

                var targeterMonikers = targeterTypes
                                            .Select(t => monikers.Type(t))
                                            .ToArray();

                return agent.GatherAllRoostSpecs(asm.FullName, targeterMonikers);
            }
            finally {
                AppDomain.Unload(appDom);
            }
        }


    }
}
