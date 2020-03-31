dotnet build -c "Release"
dotnet nuget push bin\Release\Bellight.AutoMapper.1.0.1.nupkg -k <> -s https://api.nuget.org/v3/index.json
