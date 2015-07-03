using System;
using System.IO;

namespace Cuckoo.Test.Infrastructure
{
    internal class ShadowFolder : IDisposable
    {
        string _sourcePath;
        string _fileName, _filePath;
        TempDir _dir;
        
        public ShadowFolder(string asmPath) {
            _sourcePath = asmPath;
            _fileName = Path.GetFileName(_sourcePath);

            _dir = new TempDir();

            CopyDir(Path.GetDirectoryName(_sourcePath), _dir.Path);

            _filePath = Path.Combine(_dir.Path, _fileName);
        }


        public string FolderPath { 
            get { return _dir.Path; } 
        }

        public string AssemblyPath {
            get { return _filePath; }
        }

        void IDisposable.Dispose() {
            if(_dir != null) {
                ((IDisposable)_dir).Dispose();
            }
        }

        
	

        void CopyDir(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach(var file in Directory.GetFiles(sourceDir)) {
                string targetPath = Path.Combine(targetDir, Path.GetFileName(file));

                File.Copy(file, targetPath);
                File.SetAttributes(targetPath, FileAttributes.Normal);
            }

            foreach(var directory in Directory.GetDirectories(sourceDir)) {
                CopyDir(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
            }
        }

    }



    internal class TempDir : IDisposable
    {
        DirectoryInfo _info;

        public TempDir() {
            string path = System.IO.Path.GetTempFileName();
            File.Delete(path);
            
            _info = Directory.CreateDirectory(path);
        }

        public DirectoryInfo Info {
            get { return _info; }
        }

        public string Path {
            get { return _info.FullName; }
        }

        void IDisposable.Dispose() {
            _info.Delete(true);
        }
    }

}
