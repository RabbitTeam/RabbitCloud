@echo off
set output="%~dp0%packages_publish"
echo ���·����%output%
set versionSuffix=%date:~0,4%%date:~5,2%%date:~8,2%%time:~0,2%%time:~3,2%
echo  %versionSuffix%

dotnet pack RabbitCloud.Abstractions --version-suffix %versionSuffix% -o %output%
dotnet pack config\RabbitCloud.Config.Abstractions --version-suffix %versionSuffix% -o %output%
dotnet pack config\RabbitCloud.Config --version-suffix %versionSuffix% -o %output%
dotnet pack registry\RabbitCloud.Registry.Abstractions --version-suffix %versionSuffix% -o %output%
dotnet pack registry\RabbitCloud.Registry.Consul --version-suffix %versionSuffix% -o %output%
dotnet pack rpc\RabbitCloud.Rpc.Abstractions --version-suffix %versionSuffix% -o %output%
dotnet pack rpc\RabbitCloud.Rpc --version-suffix %versionSuffix% -o %output%
dotnet pack rpc\RabbitCloud.Rpc.NetMQ --version-suffix %versionSuffix% -o %output%
dotnet pack rpc\formatters\RabbitCloud.Rpc.Formatters.Json --version-suffix %versionSuffix% -o %output%

start "" %output%
pause