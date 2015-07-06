set nuget=D:\apps\nuget\nuget
set localnugetrepo=D:\dev\nugetrepo

pushd ..\nuget 
%nuget% pack cuckoo.nuspec
copy *.nupkg "%localnugetrepo%\*.nupkg"
popd