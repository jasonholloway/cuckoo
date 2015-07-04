using Cuckoo.Gather;
using Cuckoo.Weave;
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
    public class WeaverSandbox : IDisposable
    {
        string _asmPath;
        string _folderName;
        Weaver _weaver;
        
        ShadowFolder _folder;
        AppDomain _appDomain;


        public WeaverSandbox(string asmPath, string folderName) {
            _asmPath = asmPath;
            _folderName = folderName;
            _folder = new ShadowFolder(_asmPath, _folderName);
        }



        public AssemblyDefinition GetAssembly() 
        {
            var asmResolver = new AssemblyResolver(_folder.FolderPath);

            var readerParams = new ReaderParameters() {
                AssemblyResolver = asmResolver,
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var asm = AssemblyDefinition.ReadAssembly(
                                            _folder.AssemblyPath,
                                            readerParams);

            asmResolver.RegisterAssembly(asm);

            return asm;
        }


        public IEnumerable<RoostSpec> Gather() {
            var gatherer = new Gatherer(_folder.AssemblyPath);
            return gatherer.Gather();
        }


        public void Weave(IEnumerable<RoostSpec> roostSpecs) 
        {
            var asm = GetAssembly();

            _weaver = new Weaver(
                            asm, 
                            roostSpecs, 
                            _ => { } );

            _weaver.Weave();

            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true,
            };

            asm.Write(_folder.AssemblyPath, writerParams);

            _appDomain = AppDomain.CreateDomain(
                                        "WeaverSandbox", 
                                        null, 
                                        new AppDomainSetup() { 
                                                ApplicationBase = _folder.FolderPath 
                                        });
                        
            _appDomain.Load(AssemblyName.GetAssemblyName(
                                                _folder.AssemblyPath));
        }


        public void Run(Action<AppDomain> fn) {
            fn(_appDomain);
        }


        void IDisposable.Dispose() {
            if(_appDomain != null) {                
                AppDomain.Unload(_appDomain);                
            }
        }

    }

}
