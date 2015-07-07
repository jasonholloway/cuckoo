using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using System.Linq;
using System.IO;
using Microsoft.Build.Framework;
using System.Collections.Generic;

namespace Cuckoo.IntegrationTest
{

    class Logger : ILogger
    {
        string _params;
        IEventSource _eventSource;

        List<string> _lErrors = new List<string>();
        List<string> _lMessages = new List<string>();

        public void Initialize(IEventSource eventSource) {
            _eventSource = eventSource;

            _eventSource.MessageRaised += eventSource_MessageRaised;
            _eventSource.ErrorRaised += eventSource_ErrorRaised;
            _eventSource.AnyEventRaised += eventSource_AnyEventRaised;
        }

        void eventSource_AnyEventRaised(object sender, BuildEventArgs e) {
            
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e) {
            _lErrors.Add(e.Message);
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e) {
            _lMessages.Add(e.Message);
        }

        public string Parameters {
            get {
                return _params;
            }
            set {
                _params = value;
            }
        }

        public void Shutdown() {
            if(_eventSource != null) {
                _eventSource.MessageRaised -= eventSource_MessageRaised;
                _eventSource.ErrorRaised -= eventSource_ErrorRaised;
                _eventSource.AnyEventRaised -= eventSource_AnyEventRaised;
            }
        }

        public LoggerVerbosity Verbosity {
            get {
                return LoggerVerbosity.Normal;
            }
            set {
                throw new NotImplementedException();
            }
        }
    }


    [TestClass]
    public class IntegrationTests
    {        
        [TestMethod]
        public void BuildExample() {
            var logger = new Logger();

            var pc = new ProjectCollection();
            pc.SetGlobalProperty("Configuration", "Debug");
            pc.Loggers.Add(logger);

            var result = BuildManager.DefaultBuildManager.Build(
                            new BuildParameters(pc),
                            new BuildRequestData(
                                    new ProjectInstance(@"..\..\..\Cuckoo.Example\Cuckoo.Example.csproj"),
                                    new[] { "Rebuild" })
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
