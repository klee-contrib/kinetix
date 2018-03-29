

#MSBUILD=$(reg.exe query "HKLM\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0" -v "MSBuildToolsPath" | tail -n 2 | head -n 1 | tr -s ' ' | cut -d ' ' -f 4)

echo "Building and Packaging Solution"
#$MSBUILD\msbuild.exe "KinetixCore.sln" "-t:Clean,pack" "-logger:FileLogger,Microsoft.Build.Engine;logfile=build.log"
dotnet msbuild "KinetixCore.sln" "-t:Clean,pack" "-p:Configuration=Release"

#dotnet pack "KinetixCore.sln" 

if [ $? != 0 ] 
then
	echo "Build Failed"
	exit 1
fi

echo "Build and Package Successful"


