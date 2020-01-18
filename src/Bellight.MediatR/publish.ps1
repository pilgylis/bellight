dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.MediatR.1.0.0.nupkg -k <> -s https://api.nuget.org/v3/index.json
