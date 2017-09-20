$configuration = "Release"
$base_dir = Resolve-Path "..\"
$artifacts_dir = "$base_dir\artifacts"
$build_dir = "$base_dir\build"
$source_dir = "$base_dir\src"
$working_dir = "$build_dir\working"
$projects = @(
    @{ Name = "Rabbit.Cloud.Abstractions"; SourceDir = "$source_dir\RC.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud"; SourceDir = "$source_dir\RC"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client.Abstractions"; SourceDir = "$source_dir\RC.Client.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client"; SourceDir = "$source_dir\RC.Client"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Registry.Abstractions"; SourceDir = "$source_dir\registry\RC.Registry.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Abstractions"; SourceDir = "$source_dir\discovery\RC.Discovery.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Cluster.Abstractions"; SourceDir = "$source_dir\cluster\RC.Cluster.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Cluster"; SourceDir = "$source_dir\cluster\RC.Cluster"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Facade.Abstractions"; SourceDir = "$source_dir\facade\RC.Facade.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Facade"; SourceDir = "$source_dir\facade\RC.Facade"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Facade.Formatters.Json"; SourceDir = "$source_dir\facade\RC.Facade.Formatters.Json"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Extensions.Consul"; SourceDir = "$source_dir\extensions\RC.Extensions.Consul"; ExternalNuGetDependencies = $null; UseMSBuild = $False; }
)