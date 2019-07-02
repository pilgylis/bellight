#addin nuget:?package=Nuget.Core
using NuGet;

var target = Argument("target", "Default");
var artifactsDir = "./artifacts/";
var solutionPath = "./Bellight.Core.sln";
var projectName = Argument<string>("projectName", "core");
var project = GetProject(projectName);
//var currentBranch = Argument<string>("currentBranch", GitBranchCurrent("./").FriendlyName);
//var isReleaseBuild = string.Equals(currentBranch, "master", StringComparison.OrdinalIgnoreCase);
var configuration = "Release";
var nugetApiKey = Argument<string>("nugetApiKey", "oy2itot4uet62yjdhcja6jnbk7hze6judfqhcdlyv22il4");
var nugetSource = "https://www.nuget.org/api/v2/package"; //"https://api.nuget.org/v3/index.json";

Task("Clean")
    .Does(() => {
        if (DirectoryExists(artifactsDir))
        {
            DeleteDirectory(
                artifactsDir, 
                new DeleteDirectorySettings {
                    Recursive = true,
                    Force = true
                }
            );
        }
        CreateDirectory(artifactsDir);
        DotNetCoreClean(solutionPath);
    });

Task("Publish")
    .IsDependentOn("Clean")
    .Does(() => {
        DotNetCorePublish(
            project,
            new DotNetCorePublishSettings()
            {
                Configuration = configuration,
                OutputDirectory = artifactsDir
            });
    });

Task("Pack")
    .IsDependentOn("Clean")
    .Does(() => {
        DotNetCorePack(
            project,
            new DotNetCorePackSettings
            {
                Configuration = configuration,
                OutputDirectory = artifactsDir
            } 
        );
    });

Task("Push")
    .IsDependentOn("Clean")
    .IsDependentOn("Pack")
    .Does(() => {
        var package = GetFiles($"{artifactsDir}*.nupkg").ElementAt(0);

        if(nugetApiKey==null)
            throw new ArgumentNullException(nameof(nugetApiKey), "The \"nugetApiKey\" argument must be set for this task.");

        Information($"Push {package} to {nugetSource}");

        NuGetPush(package, new NuGetPushSettings {
            Source = nugetSource,
            ApiKey = nugetApiKey
        });
    });

private bool IsNuGetPublished(FilePath packagePath) {
    var package = new ZipPackage(packagePath.FullPath);

    var latestPublishedVersions = NuGetList(
        package.Id,
        new NuGetListSettings 
        {
            Prerelease = true
        }
    );

    return latestPublishedVersions.Any(p => package.Version.Equals(new SemanticVersion(p.Version)));
}

private string GetProject(string projectName) {
    switch (projectName.ToLowerInvariant()) {
        case "configurations": return "./src/Bellight.Configurations/Bellight.Configurations.csproj";
        case "mapper": return "./src/Bellight.AutoMapper/Bellight.AutoMapper.csproj";
        default:
        case "core": return "./src/Bellight.Core/Bellight.Core.csproj";
    }
}

Information("Release build");
Task("Complete")
    .IsDependentOn("Clean")
    .IsDependentOn("Pack");

Task("Default")
    .IsDependentOn("Complete");


RunTarget(target);