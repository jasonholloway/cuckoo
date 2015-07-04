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
    public class AttributeTests : WeaveTestBase
    {
        [TestMethod]
        public void AttributesWithOptionalArgs() {
            var result = Tester.With<Atts>()
                                .Run(a => a.MethodWithOptArgAttribute());

            Assert.IsTrue(result == "blah");
        }

        [TestMethod]
        public void AttsFoundOnPrivateMethods() {
            var result = Tester.With<Atts>()
                                .Run(a => a.PrivateMethodRunner("wibblewibble"));

            Assert.IsTrue(result == "Growl");
        }

    }

}
