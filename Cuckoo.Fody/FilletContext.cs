using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Fody
{
    internal class FilletContext
    {
        public ModuleDefinition Module { get; set; }
        public ModuleDefinition CommonModule { get; set; }
        public Action<string> FnLog { get; set; }

        public event EventHandler AfterWeave;

        public void Log(string format, params object[] args) {
            FnLog(string.Format(format, args));
        }

        public void OnAfterWeave() {
            if(AfterWeave != null) AfterWeave(this, EventArgs.Empty);
        }
    }
}
