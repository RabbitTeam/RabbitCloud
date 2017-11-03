Push-Location $PSScriptRoot
. .\Settings.ps1

$suffix = "preview" + (Get-Date).ToString("yyyyMMddHHmm")
foreach ($item in $projects) {
    Write-Host "Building NuGet Package: $($item.Name)" -ForegroundColor Yellow
    dotnet pack "$($item.SourceDir)" -c Release -o $artifacts_dir --version-suffix $suffix
    Write-Host "Building NuGet Package: $($item.Name)" -ForegroundColor Yellow

    Continue;
}