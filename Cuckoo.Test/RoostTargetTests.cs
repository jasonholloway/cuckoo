using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test
{
    [TestClass]
    public class RoostTargetTests : WeavingTestBase
    {
        [TestMethod]
        public void RoostTargeterPickedUpAndRun() {
            var result = Tester.WithClass<RoostTargetClass>()
                                .Run(r => r.RoostTarget(123));

            Assert.IsTrue(result == 100);
        }



    }
}
