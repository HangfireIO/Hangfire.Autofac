var configuration = Argument("configuration", "Release");
var version = Argument<string>("buildVersion", null);
var target = Argument("target", "Default");

Task("Restore")
    .Does(()=> 
{
    DotNetCoreRestore();
});

Task("Clean")
    .Does(()=> 
{
    CleanDirectory("./build");
    StartProcess("dotnet", "clean -c:" + configuration);
});

Task("SpecifyPackageVersion")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
{
    version = AppVeyor.Environment.Build.Version;

    if (AppVeyor.Environment.Repository.Tag.IsTag)
    {
        var tagName = AppVeyor.Environment.Repository.Tag.Name;
        if(tagName.StartsWith("v"))
        {
            version = tagName.Substring(1);
        }

        AppVeyor.UpdateBuildVersion(version);
    }
});

Task("Build")
    .IsDependentOn("SpecifyPackageVersion")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(()=> 
{
    var buildSettings =  new DotNetCoreBuildSettings { Configuration = configuration };
    buildSettings.ArgumentCustomization = args => 
    {
        if (!string.IsNullOrEmpty(version)) args = args.Append("/p:Version=" + version);
        if (target == "CI") args = args.Append("/p:SourceLinkCreate=true");
        return args;
    };

    DotNetCoreBuild("Hangfire.Autofac/Hangfire.Autofac.csproj", buildSettings);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./Hangfire.Autofac.Tests/Hangfire.Autofac.Tests.csproj", new DotNetCoreTestSettings
    {
        Configuration = "Release",
        ArgumentCustomization = args => args.Append("/p:BuildProjectReferences=false")
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(()=> 
{
    CreateDirectory("build");
    
    CopyFiles(GetFiles("./Hangfire.Autofac/bin/**/*.nupkg"), "build");
    Zip("./Hangfire.Autofac/bin/" + configuration, "build/Hangfire.Autofac-" + version +".zip");
});

Task("Default")
    .IsDependentOn("Pack");

Task("CI")
    .IsDependentOn("Pack");
    
RunTarget(target);