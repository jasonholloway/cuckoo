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
                                                && typeof(IRoostPicker).IsAssignableFrom(t));       
     
            var pickers = new[] { new AttributeRoostPicker() } 
                            .Concat(pickerTypes
                                        .Select(t => (IRoostPicker)Activator.CreateInstance(t)));
            
            return pickers.SelectMany(i => i.Pick(targetAssembly))
                                .ToArray();
        }
    }
}
