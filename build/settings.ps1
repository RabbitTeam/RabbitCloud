$configuration = "Release"
$base_dir = Resolve-Path "..\"
$artifacts_dir = "$base_dir\artifacts"
$build_dir = "$base_dir\build"
$source_dir = "$base_dir\src"
$working_dir = "$build_dir\working"
$nuget_source = "https://www.myget.org/F/rabbitcloud/api/v3/index.json"
$nuget_apiKey = "xxxxxx"
$projects = @(
    @{ Name = "Rabbit.Cloud.Abstractions"; SourceDir = "$source_dir\RC.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud"; SourceDir = "$source_dir\RC"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Application.Features"; SourceDir = "$source_dir\RC.Application.Features"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Application.Abstractions"; SourceDir = "$source_dir\RC.Application.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Application"; SourceDir = "$source_dir\RC.Application"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Abstractions"; SourceDir = "$source_dir\RC.Discovery.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Consul"; SourceDir = "$source_dir\RC.Discovery.Consul"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
	@{ Name = "Rabbit.Cloud.Client.Abstractions"; SourceDir = "$source_dir\RC.Client.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
	@{ Name = "Rabbit.Cloud.Client"; SourceDir = "$source_dir\RC.Client"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
	@{ Name = "Rabbit.Cloud.Client.Http"; SourceDir = "$source_dir\RC.Client.Http"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
	@{ Name = "Rabbit.Cloud.Client.Go.Abstractions"; SourceDir = "$source_dir\RC.Client.Go.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
	@{ Name = "Rabbit.Cloud.Client.Go"; SourceDir = "$source_dir\RC.Client.Go"; ExternalNuGetDependencies = $null; UseMSBuild = $False; }
)