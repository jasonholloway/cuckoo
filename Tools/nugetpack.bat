set sourcedir=%1
set nuget=D:\apps\nuget\nuget
set localnugetrepo=D:\dev\nugetrepo

pushd %sourcedir% 
%nuget% pack cuckoo.nuspec -verbosity quiet
copy *.nupkg "%localnugetrepo%\*.nupkg"
popd