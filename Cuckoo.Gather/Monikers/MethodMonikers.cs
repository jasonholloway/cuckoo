﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather.Monikers
{    
    public interface IMethodMoniker
    {
        ITypeMoniker DeclaringType { get; }
        string Name { get; }
        string FullName { get; }
        ITypeMoniker[] ArgTypes { get; }
    }


    [Serializable]
    public class MethodDef : IMethodMoniker
    {
        public ITypeMoniker DeclaringType { get; private set; }
        public string Name { get; private set; }
        public ITypeMoniker[] ArgTypes { get; private set; }
        public int MetadataToken { get; private set; }

        public MethodDef(ITypeMoniker decType, string name, ITypeMoniker[] argTypes, int metadataToken) {
            DeclaringType = decType;
            Name = name;
            ArgTypes = argTypes;
            MetadataToken = metadataToken;
        }

        public string FullName {
            get { return string.Format("{0}.{1}()", DeclaringType.FullName, Name); }
        }
    }

    [Serializable]
    public class GenMethodSpec : IMethodMoniker
    {
        public IMethodMoniker BaseMethod { get; private set; }
        public ITypeMoniker[] GenArgs { get; private set; }

        public GenMethodSpec(IMethodMoniker baseMethod, ITypeMoniker[] genArgs) {
            BaseMethod = baseMethod;
            GenArgs = genArgs;
        }

        public ITypeMoniker DeclaringType {
            get { return BaseMethod.DeclaringType; }
        }

        public string Name {
            get { return BaseMethod.Name; }
        }

        public ITypeMoniker[] ArgTypes {
            get { return BaseMethod.ArgTypes; } //this won't work nicely for gen params, which should be resolved
        }

        public string FullName {
            get { return string.Format("{0}.{1}()", DeclaringType.FullName, Name); } //INCOMPLETE!!!
        }
    }



}
