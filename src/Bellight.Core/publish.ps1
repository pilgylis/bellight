$ver = $args[0]
$key = $args[1]
dotnet build -c "Release" -p:Version=$ver
dotnet nuget push bin\Release\Bellight.Core.$ver.nupkg -k $key -s https://api.nuget.org/v3/index.json
