﻿using Cuckoo.Test.Infrastructure;
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
    public class GenericMethodTests : WeavingTestBase
    {

        [TestMethod]
        public void CuckooOnMethodWithGenericArgs() {
            int r = Tester.WithClass<GenericArgs>()
                            .Run(g => g.MethodWithGenericArgs<string, float>("", 2F));


            Assert.IsTrue(r == 999);
        }


        [TestMethod]
        public void CuckooOnMethodWithGenericResult() {
            int r = Tester.WithClass<GenericArgs>()
                            .Run(g => g.MethodWithGenericResult<int>(99));

            Assert.IsTrue(r == default(int));
        }

        [TestMethod]
        public void CuckoosCooperateOnMethodWithGenericArgs() {
            int r = Tester.WithClass<GenericArgs>()
                            .Run(g => g.TreblyCuckooedMethodWithGenericArgs(1, 2, 3));

            Assert.IsTrue(r == 90);
        }


    }
}
