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
    @{ Name = "Rabbit.Cloud.Client.Proxy"; SourceDir = "$source_dir\RC.Client.Proxy"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Abstractions"; SourceDir = "$source_dir\RC.Discovery.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Consul"; SourceDir = "$source_dir\RC.Discovery.Consul"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Discovery.Memory"; SourceDir = "$source_dir\RC.Discovery.Memory"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client.Grpc"; SourceDir = "$source_dir\RC.Client.Grpc"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client.Grpc.Proxy"; SourceDir = "$source_dir\RC.Client.Grpc.Proxy"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Grpc.Abstractions"; SourceDir = "$source_dir\RC.Grpc.Abstractions"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Grpc.Client"; SourceDir = "$source_dir\RC.Grpc.Client"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Grpc.Server"; SourceDir = "$source_dir\RC.Grpc.Server"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Grpc.Fluent"; SourceDir = "$source_dir\RC.Grpc.Fluent"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client.Breaker"; SourceDir = "$source_dir\RC.Client.Breaker"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Client.LoadBalance"; SourceDir = "$source_dir\RC.Client.LoadBalance"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Server.Grpc"; SourceDir = "$source_dir\RC.Server.Grpc"; ExternalNuGetDependencies = $null; UseMSBuild = $False; },
    @{ Name = "Rabbit.Cloud.Server.Monitor"; SourceDir = "$source_dir\RC.Server.Monitor"; ExternalNuGetDependencies = $null; UseMSBuild = $False; }
)