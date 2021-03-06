﻿using System;

namespace Cuckoo.Impl
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class CuckooedAttribute : Attribute
    {
        public string InnerMethodName { get; private set; }

        public CuckooedAttribute(string innerMethodName) {
            InnerMethodName = innerMethodName;
        }
    }
}
