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
        public static Assembly Reweave(string inputPath) 
        {

            var path = inputPath.Replace(".dll", ".Rewoven.dll");

            var inputPdbPath = inputPath.Replace(".dll", ".pdb");
            var pdbPath = inputPdbPath.Replace(".pdb", ".Rewoven.pdb");

            File.Copy(inputPath, path, true);
            File.Copy(inputPdbPath, pdbPath, true);




            var readerParams = new ReaderParameters() {
                AssemblyResolver = new DefaultAssemblyResolver(),
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var module = ModuleDefinition
                            .ReadModule(path, readerParams);


            string newName = module.Name.Replace(".dll", ".Rewoven.dll");

            module.Assembly.Name = new AssemblyNameDefinition(newName, new Version());
            module.Name = newName;


            var weaver = new ModuleWeaver() {
                AssemblyFilePath = path,
                ModuleDefinition = module,
                LogInfo = _ => { }
            };

            weaver.Execute();


            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true
            };



            module.Write(path, writerParams);

            return Assembly.LoadFile(path);
        }


        public static Assembly Reweave(Assembly inputAssembly, string outputPath = null) 
        {
            return Reweave(inputAssembly.Location);
        }

    }

}
