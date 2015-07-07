using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using System.Linq;
using System.IO;

namespace Cuckoo.IntegrationTest
{
    [TestClass]
    public class IntegrationTests
    {        
        [TestMethod]
        public void BuildExample() {
            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");

            var result = BuildManager.DefaultBuildManager.Build(
                            new BuildParameters(pc),
                            new BuildRequestData(
                                    new ProjectInstance(@"..\..\..\Cuckoo.Example\Cuckoo.Example.csproj"), 
                                    new[] { "Rebuild" } )                                    
                            );

            Assert.IsTrue(result.OverallResult == BuildResultCode.Success);
        }

        [TestMethod]
        public void RunExample() {
            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");

            var result = BuildManager.DefaultBuildManager.Build(
                            new BuildParameters(pc),
                            new BuildRequestData(
                                    new ProjectInstance(@"..\..\..\Cuckoo.Example\Cuckoo.Example.csproj"),
                                    new[] { "Build" })
                            );

            Assert.IsTrue(result.OverallResult == BuildResultCode.Success);

            var item = result.ResultsByTarget["Build"].Items.First();

            var appDom = AppDomain.CreateDomain(
                                    "RunWithCustomTargeter", 
                                    null,
                                    new AppDomainSetup() {
                                        ApplicationBase = Path.GetDirectoryName(item.ItemSpec)
                                    });

            try {
                appDom.ExecuteAssembly(item.ItemSpec);
            }
            catch {
                throw;
            }
            finally {
                AppDomain.Unload(appDom);
            }
        }


    }
}
