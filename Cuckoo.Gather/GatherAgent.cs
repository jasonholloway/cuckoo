using System;
using System.Linq;

namespace Cuckoo.Gather
{
    public class GatherAgent : MarshalByRefObject
    {
        public RoostSpec[] GatherAllRoostTargets(string targetAssemblyName) 
        {
            var targetAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                    .First(a => a.FullName == targetAssemblyName);

            var targeterTypes = targetAssembly.GetTypes()
                                    .Where(t => ! t.IsAbstract 
                                                && typeof(IRoostPicker).IsAssignableFrom(t));
            
            var targeters = new[] { 
                                new AttributeRoostTargeter() 
                            } 
                            .Concat(targeterTypes
                                        .Select(t => (IRoostPicker)Activator.CreateInstance(t)));
            
            return targeters.SelectMany(i => i.PickRoosts(targetAssembly))
                                .ToArray();
        }
    }
}
