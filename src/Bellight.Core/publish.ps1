dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.Core.1.0.2.nupkg -k <> -s https://api.nuget.org/v3/index.json
