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
        Weaver _weaver;
        
        ShadowFolder _folder;
        AppDomain _appDomain;

        bool _isInitialized = false;

        public WeaverSandbox(string asmPath) {
            _asmPath = asmPath;
        }

        public void Init() {
            if(_isInitialized) return;

            _isInitialized = true;
            
            var gatherer = new Gatherer(_asmPath);
            var roostSpecs = gatherer.Gather();
            
            _folder = new ShadowFolder(_asmPath);

            var asmResolver = new AssemblyResolver(_folder.FolderPath);

            var readerParams = new ReaderParameters() {
                AssemblyResolver = asmResolver,
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var asm = AssemblyDefinition.ReadAssembly(
                                            _folder.AssemblyPath, 
                                            readerParams );

            asmResolver.RegisterAssembly(asm);

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
