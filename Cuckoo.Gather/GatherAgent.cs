using System;
using System.Linq;
using System.Reflection;

namespace Cuckoo.Gather
{
    public class GatherAgent : MarshalByRefObject
    {
        public void LoadAssemblies(string[] asmPaths) {
            foreach(var asmPath in asmPaths) {
                var asmName = AssemblyName.GetAssemblyName(asmPath);
                Assembly.Load(asmName);
            }
        }

        public RoostSpec[] GatherAllRoostSpecs(string targetAsmPath) 
        {
            var asmName = AssemblyName.GetAssemblyName(targetAsmPath);            
            var targetAsm = Assembly.Load(asmName);

            var pickerTypes = targetAsm.GetTypes()
                                    .Where(t => ! t.IsAbstract
                                                && !t.IsGenericTypeDefinition
                                                && typeof(IRoostTargeter).IsAssignableFrom(t));       
     
            var pickers = new[] { new AttributeRoostPicker() } 
                            .Concat(pickerTypes
                                        .Select(t => (IRoostTargeter)Activator.CreateInstance(t)));
            
            var roostSpecs = pickers.SelectMany(i => i.TargetRoosts(targetAsm))
                                        .ToArray();

            return roostSpecs;
        }
    }
}
