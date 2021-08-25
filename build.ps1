# restore and builds all projects as release.
# creates NuGet package at \artifacts
dotnet --version

$versionPrefix = "4.5.0" # Also update version for minor versions in appveyor.yml 
$versionSuffix = ""
$versionFile = $versionPrefix + "." + ${env:APPVEYOR_BUILD_NUMBER}
if ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
    $versionPrefix = $versionFile
    $versionSuffix = "PR" + $env:APPVEYOR_PULL_REQUEST_NUMBER
}

msbuild /t:restore,build /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /maxcpucount /p:VersionPrefix=$versionPrefix /p:VersionSuffix=$versionSuffix /p:FileVersion=$versionFile /p:ContinuousIntegrationBuild=true /p:EmbedUntrackedSources=true /verbosity:minimal
if (-Not $LastExitCode -eq 0) {
    exit $LastExitCode 
}

exit $LastExitCode
