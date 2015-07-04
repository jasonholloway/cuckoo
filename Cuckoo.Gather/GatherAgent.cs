using System;
using System.Linq;

namespace Cuckoo.Gather
{
    public class GatherAgent : MarshalByRefObject
    {
        public RoostSpec[] GatherAllRoostSpecs(string targetAssemblyName) 
        {
            var targetAssembly = AppDomain.CurrentDomain.Load(targetAssemblyName);

            var pickerTypes = targetAssembly.GetTypes()
                                    .Where(t => ! t.IsAbstract
                                                && !t.IsGenericTypeDefinition
                                                && typeof(IRoostTargeter).IsAssignableFrom(t));       
     
            var pickers = new[] { new AttributeRoostPicker() } 
                            .Concat(pickerTypes
                                        .Select(t => (IRoostTargeter)Activator.CreateInstance(t)));
            
            return pickers.SelectMany(i => i.TargetRoosts(targetAssembly))
                                .ToArray();
        }
    }
}
