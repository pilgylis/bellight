dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.MessageBus.Amqp.1.0.0.nupkg -k oy2itot4uet62yjdhcja6jnbk7hze6judfqhcdlyv22il4 -s https://api.nuget.org/v3/index.json
