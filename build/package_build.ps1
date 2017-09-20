Push-Location $PSScriptRoot
. .\Settings.ps1

foreach ($item in $projects) {
    Write-Host "Building NuGet Package: $($item.Name)" -ForegroundColor Yellow
    dotnet pack "$($item.SourceDir)" -c Release -o $artifacts_dir --version-suffix (Get-Date).ToString("yyyyMMddHHmm")
    Write-Host "Building NuGet Package: $($item.Name)" -ForegroundColor Yellow

    Continue;
}