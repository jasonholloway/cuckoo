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

        public WeaverSandbox(string asmPath, Weaver weaver) {
            _asmPath = asmPath;
            _weaver = weaver;
        }

        public void Init() {
            _folder = new ShadowFolder(_asmPath);

            var asmResolver = new AssemblyResolver(_folder.FolderPath);

            var readerParams = new ReaderParameters() {
                AssemblyResolver = asmResolver,
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var module = ModuleDefinition
                            .ReadModule(_folder.AssemblyPath, readerParams);

            asmResolver.RegisterAssembly(module.Assembly);

            _weaver.Init(
                        module, 
                        _folder.AssemblyPath, 
                        _ => { } );

            _weaver.Weave();

            var writerParams = new WriterParameters() {
                SymbolWriterProvider = new PdbWriterProvider(),
                WriteSymbols = true,
            };

            module.Write(_folder.AssemblyPath, writerParams);

            _appDomain = AppDomain.CreateDomain(
                                        "WeaverSandbox", 
                                        null, 
                                        new AppDomainSetup() { 
                                                ApplicationBase = _folder.FolderPath 
                                        });
                        
            _appDomain.Load(AssemblyName.GetAssemblyName(_folder.AssemblyPath));
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
