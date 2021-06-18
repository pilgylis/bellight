$ver = $args[0]
$key = $args[1]
dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.MessageBus.Amqp.$ver.nupkg -k $key -s https://api.nuget.org/v3/index.json
