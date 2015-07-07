﻿using Cuckoo.Gather.Specs;
using System;
using System.Linq;
using System.Reflection;

namespace Cuckoo.Gather
{
    public class GatherAgent : MarshalByRefObject
    {
        Assembly _targetAsm;

        public void Init(AssemblyLocator locator) {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(
                (o, r) => {
                    var path = locator.LocateAssembly(r.Name);
                    var asmName = AssemblyName.GetAssemblyName(path);
                    return Assembly.Load(asmName);
                });
        }
        

        public RoostSpec[] GatherAllRoostSpecs(string targetAsmName) 
        {            
            _targetAsm = Assembly.Load(targetAsmName);

            var pickerTypes = _targetAsm.GetTypes()
                                    .Where(t => ! t.IsAbstract
                                                && !t.IsGenericTypeDefinition
                                                && typeof(IRoostTargeter).IsAssignableFrom(t));       
     
            var targeters = new[] { 
                                new AttributeTargeter() 
                            } 
                            .Concat(pickerTypes
                                        .Select(t => (IRoostTargeter)Activator.CreateInstance(t)));
            
            var targets = targeters
                            .SelectMany(i => i.TargetRoosts(_targetAsm));
            
            foreach(var t in targets) {
                ValidateTarget(t);
            }

            var specs = targets
                            .Select(t => new RoostSpec(
                                                t.TargetMethod, 
                                                t.HatcherCtor,
                                                t.HatcherCtorArgs,
                                                t.HatcherCtorNamedArgs.ToArray()
                                                ));
            
            return specs.ToArray();
        }



        void ValidateTarget(RoostTarget target) {
            if(target.TargetMethod.Module.Assembly != _targetAsm) {
                throw new CuckooGatherException(
                            "Can't target method outside of local assembly! {0}.{1} isn't from round here.", 
                            target.TargetMethod.DeclaringType.FullName,
                            target.TargetMethod.Name );
            }

            ValidateHatcher(target);
        }


        void ValidateHatcher(RoostTarget target) 
        {
            var hatcherType = target.HatcherCtor.DeclaringType;
            var hatcherName = string.Format("{0}.{1}", hatcherType.FullName, target.HatcherCtor.Name);

            if(hatcherType.IsAbstract) {
                throw new CuckooGatherException(
                            "Specified CuckooHatcher {0} is abstract, and therefore illegal!",
                            hatcherName
                            );
            }

            if(hatcherType.IsGenericTypeDefinition || hatcherType.IsArray) {
                throw new CuckooGatherException(
                            "Found CuckooHatcher of unacceptable type instantiation!"
                            );
            }

            var rParams = target.HatcherCtor.GetParameters();

            if(rParams.Length != target.HatcherCtorArgs.Length) {
                throw new CuckooGatherException(
                            "Wrong number of args specified for {0}",
                            hatcherName);
            }

            var zippedArgs = rParams.Zip(target.HatcherCtorArgs,
                                            (p, a) => new { Param = p, Arg = a });

            foreach(var z in zippedArgs) {
                if(!z.Param.ParameterType.IsAssignableFrom(z.Arg.GetType())) {
                    throw new CuckooGatherException(
                                "Argument of bad type passed for {0}",
                                hatcherName);
                }
            }


            var namedArgs = target.HatcherCtorNamedArgs.Select(
                                    (kv) => new { Name = kv.Key, Arg = kv.Value });

            foreach(var n in namedArgs) {
                var member = hatcherType
                                .GetMember(
                                    n.Name, 
                                    MemberTypes.Field | MemberTypes.Property, 
                                    BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault();

                if(member == null) {
                    throw new CuckooGatherException(
                                "Bad named arg passed for {0}. Member {1} can't be found!",
                                hatcherType.FullName,
                                n.Name);
                }
                
                var type = member is PropertyInfo
                            ? ((PropertyInfo)member).PropertyType
                            : ((FieldInfo)member).FieldType;

                if(!type.IsAssignableFrom(n.Arg.GetType())) {
                    throw new CuckooGatherException(
                                "Named arg of bad type passed to {0}. {1} should be {2}!",
                                hatcherName,
                                member.Name,
                                type.FullName);
                }
            }

        }


    }
}
