dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.MessageBus.Amqp.1.0.3.nupkg -k <key here> -s https://api.nuget.org/v3/index.json
