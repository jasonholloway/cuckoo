# valid versions are [2.0, 3.5, 4.0]
$dotNetVersion = "4.0"
$regKey = "HKLM:\software\Microsoft\MSBuild\ToolsVersions\$dotNetVersion"
$regProperty = "MSBuildToolsPath"


$msbuildExe = join-path -path (Get-ItemProperty $regKey).$regProperty -childpath "msbuild.exe"




# Now run on int test proj

cd "..\Cuckoo.IntegrationTest"

&$msbuildExe -t:rebuild "Cuckoo.IntegrationTest.csproj"