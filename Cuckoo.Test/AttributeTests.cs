using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Weave;
using Cuckoo;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;

namespace Cuckoo.Test
{
    [TestClass]
    public class AttributeTests : WeavingTestBase2
    {

        [TestMethod]
        public void CuckooAttributesWithOptionalArgs() {
            var result = Tester.With<Atts>()
                                .Run(a => a.MethodWithOptArgAttribute());

            Assert.IsTrue(result == "blah");
        }

    }
}
