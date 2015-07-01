﻿using System;
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
    public class PropertyTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooOnPropertyGetter() {
            var result = Tester.WithClass<Properties>()
                                .Run(p => p.Prop);

            Assert.IsTrue(result == 17);
        }

        [TestMethod]
        public void CuckooOnPropertySetter() {
            var result = Tester.Static()
                                .Run(() => new Properties() { Prop = 22 }.Value);

            Assert.IsTrue(result == 13);
        }
        


    }
}