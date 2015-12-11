for /d %%G in ("..\packages\Cuckoo*") do rd /s /q "%%~G"
nuget restore ..\Cuckoo.sln -source D:\dev\nugetrepo\