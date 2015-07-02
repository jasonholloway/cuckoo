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
    public class RoostTargetTests
    {
        [TestMethod]
        public void RoostTargetPickedUpAndRun() {
            int oldCtorCount = SimpleRoostTargeter.InstanceCount;
            int oldRunCount = SimpleRoostTargeter.RunCount;

            var rewovenAsm = WeaverRunner.Reweave(typeof(SimpleRoostTargeter).Assembly);

            Assert.IsTrue(oldCtorCount == (SimpleRoostTargeter.InstanceCount - 1));
            Assert.IsTrue(oldCtorCount == (SimpleRoostTargeter.RunCount - 1));
        }



    }
}
