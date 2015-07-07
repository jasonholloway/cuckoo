using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Cuckoo.Gather
{
    [Serializable]
    public class CuckooGatherException : Exception, ISerializable
    {
        public CuckooGatherException(string message) 
            : base(message) { }

        public CuckooGatherException(string format, params object[] args) 
            : base(string.Format(format, args)) { }

        public CuckooGatherException(SerializationInfo info, StreamingContext context) :
            this((string)info.GetValue("message", typeof(string))) { }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("message", this.Message);
        }
    }
}
