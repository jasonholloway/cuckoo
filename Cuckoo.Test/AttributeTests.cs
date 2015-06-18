using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Cuckoo.Fody;
using Cuckoo;
using Cuckoo.Test.Infrastructure;
using Cuckoo.TestAssembly;

namespace Cuckoo.Test
{
    [TestClass]
    public class AttributeTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooAttributesWithOptionalArgs() {
            var result = Tester.WithClass<Atts>()
                                .Run(a => a.MethodWithOptArgAttribute());

            Assert.IsTrue(result == "blah");
        }

    }
}
