using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.TestAssembly.Cuckoos
{
    public class AttPropsCuckooAttribute : CuckooAttribute
    {
        public AttPropsCuckooAttribute() {

        }

        public byte Byte { get; set; }
        public char Char { get; set; }
        public int Int { get; set; }
        public uint UInt { get; set; }
        public long Long { get; set; }
        public ulong ULong { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public string String { get; set; }
        public Type Type { get; set; }
        
        public override void Call(ICall call) {
            base.Call(call);

            call.ReturnValue = new object[] { 
                Byte, Char, Int, UInt, Long, ULong, Float, Double, String, Type
            };
        }
    }
}
