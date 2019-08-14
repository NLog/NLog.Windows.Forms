function install-dotnet-core {
    if ($env:APPVEYOR -eq "True") {
        $env:DOTNET_INSTALL_DIR = Join-Path "$(Convert-Path "$PSScriptRoot")" ".dotnetcli"
        mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
        Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        & $installScript -Version "$env:DOTNET_VERSION" -InstallDir "$env:DOTNET_INSTALL_DIR"
    }
}
install-dotnet-core