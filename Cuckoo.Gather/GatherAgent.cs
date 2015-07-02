using System;
using System.Linq;

namespace Cuckoo.Gather
{
    public class GatherAgent : MarshalByRefObject
    {
        public TargetRoost[] GatherAllRoostTargets(string targetAssemblyName) 
        {
            var targetAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .First(a => a.FullName == targetAssemblyName);

            var targeterTypes = targetAssembly.GetTypes()
                                    .Where(t => ! t.IsAbstract 
                                                && typeof(IRoostTargeter).IsAssignableFrom(t));
            
            var targeters = new[] { 
                                new AttributeRoostTargeter() 
                            } 
                            .Concat(targeterTypes
                                        .Select(t => (IRoostTargeter)Activator.CreateInstance(t)));
            
            return targeters.SelectMany(i => i.GetTargets(targetAssembly))
                                .ToArray();
        }
    }
}
