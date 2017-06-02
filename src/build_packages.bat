@echo off
set output="%~dp0%packages_publish"
echo Êä³öÂ·¾¶£º%output%

dotnet pack RabbitCloud.Abstractions -o %output%
dotnet pack config\RabbitCloud.Config.Abstractions -o %output%
dotnet pack config\RabbitCloud.Config -o %output%
dotnet pack registry\RabbitCloud.Registry.Abstractions -o %output%
dotnet pack registry\RabbitCloud.Registry.Consul -o %output%
dotnet pack rpc\RabbitCloud.Rpc.Abstractions -o %output%
dotnet pack rpc\RabbitCloud.Rpc -o %output%
dotnet pack rpc\RabbitCloud.Rpc.NetMQ -o %output%
dotnet pack rpc\formatters\RabbitCloud.Rpc.Formatters.Json -o %output%

start "" %output%
pause