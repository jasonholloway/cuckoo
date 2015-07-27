using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Cuckoo.Gather.Targeters
{
    /// <summary>
    /// Scans the target assembly for concrete IRoostTargeter implementations; constructs and runs these if found; and aggregates their output targets.
    /// </summary>
    public class CascadeTargeter : IRoostTargeter 
    {
        public IEnumerable<RoostTarget> TargetRoosts(Assembly assembly) 
        {
            var targeterTypes = assembly.GetTypes()
                                            .Where(t => !t.IsAbstract
                                                        && !t.IsGenericTypeDefinition
                                                        && typeof(IRoostTargeter).IsAssignableFrom(t));

            var targeters = targeterTypes
                                .Select(t => (IRoostTargeter)Activator.CreateInstance(t));

            return targeters
                      .SelectMany(i => i.TargetRoosts(assembly));
        }
    }
}
