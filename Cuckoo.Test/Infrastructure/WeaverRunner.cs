using Cuckoo.Fody;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cuckoo.Test.Infrastructure
{
    public static class WeaverRunner
    {
        public static Assembly Reweave(string inputPath, string outputPath = null) 
        {
            var asmResolver = new DefaultAssemblyResolver();

            var readerParams = new ReaderParameters() {
                AssemblyResolver = asmResolver,
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var module = ModuleDefinition
                            .ReadModule(inputPath, readerParams);

            string newName = module.Name + ".Rewoven";

            module.Assembly.Name = new AssemblyNameDefinition(newName, new Version());
            module.Name = newName;


            var weaver = new ModuleWeaver() {
                AssemblyFilePath = inputPath,
                ModuleDefinition = module,
                LogInfo = _ => { }
            };

            weaver.Execute();


            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true
            };


            if(outputPath == null) {
                outputPath = inputPath.Replace(".dll", ".Rewoven.dll");
            }

            module.Write(outputPath, writerParams);

            return Assembly.LoadFile(outputPath);
        }


        public static Assembly Reweave(Assembly inputAssembly, string outputPath = null) 
        {
            return Reweave(inputAssembly.Location);
        }

    }

}
