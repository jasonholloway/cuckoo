using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuckooConsumer
{
    public static class CuckooCount
    {
        public static ConcurrentQueue<DateTime> CallTimes = new ConcurrentQueue<DateTime>();
    }
}
