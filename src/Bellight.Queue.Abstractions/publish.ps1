dotnet build -c "Release"
..\Tools\nuget.exe push -Source "abiding-feed" -ApiKey VSTS bin\Release\Bellight.Framework.1.8.3.nupkg
