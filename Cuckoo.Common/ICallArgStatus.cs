﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Common
{
    public interface ICallArgStatus
    {
        bool HasChanged { get; }
    }
}
