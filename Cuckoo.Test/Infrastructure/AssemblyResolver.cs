using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    internal class AssemblyResolver : DefaultAssemblyResolver
    {
        public AssemblyResolver(string folderPath) 
        {
            foreach(var dirPath in GetSearchDirectories()) {
                RemoveSearchDirectory(dirPath);
            }

            AddSearchDirectory(folderPath);
        }

        public new void RegisterAssembly(AssemblyDefinition assembly) {
            base.RegisterAssembly(assembly);
        }
    }



}
