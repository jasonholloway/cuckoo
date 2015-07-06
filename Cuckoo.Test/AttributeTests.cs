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
using System.Collections.Generic;

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


        [TestMethod]
        public void TypeArgsInAttCtors() {
            var result = Tester.With<Atts>()
                                .Run(a => a.TypeArgInAttCtor());

            Assert.IsTrue(result == typeof(Atts).FullName);
        }

        [TestMethod]
        public void VariousArgTypesInAttCtors() {
            object[] result = Tester.With<Atts>()
                                        .Run(a => a.VariousAttArgs());

            object[] ideal = typeof(Atts).GetMethod("VariousAttArgs")
                                            .GetCustomAttributesData()
                                            .First()
                                                .ConstructorArguments
                                                    .Select(a => a.Value)
                                                    .ToArray();

            Assert.IsTrue(result.SequenceEqual(ideal));
        }

        [TestMethod]
        public void VariousArgTypesInAttProps() {
            object[] result = Tester.With<Atts>()
                                        .Run(a => a.VariousAttProps());

            object[] ideal = typeof(Atts).GetMethod("VariousAttProps")
                                            .GetCustomAttributesData()
                                            .First()
                                                .NamedArguments
                                                    .Select(a => a.TypedValue.Value)
                                                    .ToArray();

            Assert.IsTrue(result.SequenceEqual(ideal));
        }

        [TestMethod]
        public void ParamsArrayAsArgInAttCtor() {
            object[] result = Tester.With<Atts>()
                                        .Run(a => a.AttArgsByParamsArray());

            object[] ideal = (typeof(Atts).GetMethod("AttArgsByParamsArray")
                                            .GetCustomAttributesData()
                                            .First()
                                                .ConstructorArguments
                                                .First().Value as IEnumerable<CustomAttributeTypedArgument>)
                                                    .Select(a => a.Value)
                                                    .ToArray();

            Assert.IsTrue(result.SequenceEqual(ideal));
        }


        [TestMethod]
        public void OptionalArgsOnAttCtors() {
            string[] result = Tester.With<Atts>()
                                        .Run(a => a.OptionalAttArgs());

            Assert.IsTrue(result.SequenceEqual(new[] { "plop", "pork", "pig" }));
        }

    }

}
