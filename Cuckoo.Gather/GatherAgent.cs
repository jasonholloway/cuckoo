using System;
using System.Linq;
using System.Reflection;

namespace Cuckoo.Gather
{
    internal class GatherAgent : MarshalByRefObject
    {        
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
            var targetAsm = Assembly.Load(targetAsmName);

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
