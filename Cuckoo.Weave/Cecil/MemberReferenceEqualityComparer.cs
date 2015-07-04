using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Weave.Cecil
{
    internal class MemberReferenceEqualityComparer 
        : IEqualityComparer<MemberReference>
    {
        public static readonly MemberReferenceEqualityComparer Default = new MemberReferenceEqualityComparer();

        public bool Equals(MemberReference x, MemberReference y) {
            return x.FullName == y.FullName
                    && x.Module.FullyQualifiedName == y.Module.FullyQualifiedName
                    && x.DeclaringType.FullName == y.DeclaringType.FullName;
        }

        public int GetHashCode(MemberReference obj) {
            return obj.FullName.GetHashCode() ^ obj.Module.FullyQualifiedName.GetHashCode();
        }
    }
}
