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
    public class WeaverSandbox : IDisposable
    {
        string _asmPath;
        ModuleWeaver _weaver;

        ShadowFolder _folder;
        AppDomain _appDomain;

        public WeaverSandbox(string asmPath, ModuleWeaver weaver) {
            _asmPath = asmPath;
            _weaver = weaver;
        }

        public void Init() {
            _folder = new ShadowFolder(_asmPath);

            var readerParams = new ReaderParameters() {
                AssemblyResolver = new DefaultAssemblyResolver(),
                SymbolReaderProvider = new PdbReaderProvider(),
                ReadSymbols = true,
                ReadingMode = ReadingMode.Immediate
            };

            var module = ModuleDefinition
                            .ReadModule(_folder.AssemblyPath, readerParams);

            _weaver.AssemblyFilePath = _folder.AssemblyPath;
            _weaver.ModuleDefinition = module;
            _weaver.LogInfo = _ => { };

            _weaver.Execute();

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
                _appDomain = null;
            }

            if(_folder != null) {
                ((IDisposable)_folder).Dispose();
                _folder = null;
            }
        }
    }

}
