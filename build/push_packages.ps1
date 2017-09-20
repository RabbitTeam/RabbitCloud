Push-Location $PSScriptRoot
. .\Settings.ps1

nuget.exe push $artifacts_dir\*.nupkg $nuget_apiKey -source $nuget_source