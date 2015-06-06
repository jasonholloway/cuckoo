using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class WeaveContext
    {
        public event EventHandler AfterWeave;

        public void OnAfterWeave() {
            if(AfterWeave != null) AfterWeave(this, EventArgs.Empty);
        }
    }
}
