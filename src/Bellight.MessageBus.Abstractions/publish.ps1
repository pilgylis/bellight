dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.MessageBus.Abstractions.1.0.2.nupkg -k <key here> -s https://api.nuget.org/v3/index.json
