#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

// Cake Addins
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var VERSION = "0.6.1";
var NUGET_SUFIX = ".0";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var artifacts = new [] {
    new Artifact {
        Version = VERSION + NUGET_SUFIX,
        NativeVersion = VERSION,
        ReleaseNotes = new string [] {
            string.Format("SoLoader for Android v{0}", VERSION)
        },
        SolutionPath = "./soloader-android.sln",
        AssemblyInfoPath = "./Naxam.SoLoader.Droid/Properties/AssemblyInfo.cs",
        NuspecPath = "./soloader.nuspec",
        DownloadUrl = "https://repo1.maven.org/maven2/com/facebook/soloader/soloader/{0}/soloader-{0}.aar",
        JarPath = "./Naxam.SoLoader.Droid/Jars/soloader.aar",
        Dependencies = new NuSpecDependency[] {}
    }
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Downloads")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        var downloadUrl = string.Format(artifact.DownloadUrl, artifact.NativeVersion);
        var jarPath = string.Format(artifact.JarPath, artifact.NativeVersion);

        DownloadFile(downloadUrl, jarPath);
    }
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory("**/*/packages");

    CleanDirectory("./nugets/*");

    var nugetPackages = GetFiles("./nugets/*.nupkg");

    foreach (var package in nugetPackages)
    {
        DeleteFile(package);
    }
});

Task("UpdateVersion")
    .Does(() => 
{
    foreach(var artifact in artifacts) {
        ReplaceRegexInFiles(artifact.AssemblyInfoPath, "\\[assembly\\: AssemblyVersion([^\\]]+)\\]", string.Format("[assembly: AssemblyVersion(\"{0}\")]", artifact.Version));
    }
});

Task("Build")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetRestore(artifact.SolutionPath);
        MSBuild(artifact.SolutionPath, settings => {
            settings.ToolVersion = MSBuildToolVersion.VS2019;
            settings.SetConfiguration(configuration);
        });
    }
});

Task("Pack")
    .Does(() =>
{
    foreach(var artifact in artifacts) {
        NuGetPack(artifact.NuspecPath, new NuGetPackSettings {
            Version = artifact.Version,
            Dependencies = artifact.Dependencies,
            ReleaseNotes = artifact.ReleaseNotes,
            OutputDirectory = "./nugets"
        });
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    // .IsDependentOn("Downloads")
    .IsDependentOn("UpdateVersion")
    .IsDependentOn("Build")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

class Artifact {
    public string Version { get; set; }
    public string NativeVersion { get; set; }

    public string AssemblyInfoPath { get; set; }

    public string SolutionPath { get; set; }

    public string DownloadUrl  { get; set; }

    public string JarPath { get; set; }

    public string NuspecPath { get; set; }

    public string[] ReleaseNotes { get; set; }

    public NuSpecDependency[] Dependencies { get; set; }
}