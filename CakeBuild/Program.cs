using Cake.Common;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Clean;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Core;
using Cake.Frosting;
using Cake.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using Vintagestory.API.Common;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public static readonly string[] ProjectNames = new string[] { "sanctuaries", "shackles", "snitches" };
    public string BuildConfiguration { get; set; }
    public string[] Versions { get; }
    public string[] Names { get; }
    public bool SkipJsonValidation { get; set; }

    public BuildContext(ICakeContext context)
        : base(context)
    {
        Versions = new string[ProjectNames.Length];
        Names = new string[ProjectNames.Length];
        for (int i = 0; i < ProjectNames.Length; i++) {
            BuildConfiguration = context.Argument("configuration", "Release");
            SkipJsonValidation = context.Argument("skipJsonValidation", false);
            var modInfo = context.DeserializeJsonFromFile<ModInfo>($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/modinfo.json");
            Versions[i] = modInfo.Version;
            Names[i] = modInfo.ModID;
        }
    }
}

[TaskName("ValidateJson")]
public sealed class ValidateJsonTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        for (int i = 0; i < BuildContext.ProjectNames.Length; i++)
        {
            if (context.SkipJsonValidation)
            {
                return;
            }
            var jsonFiles = context.GetFiles($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/assets/**/*.json");
            foreach (var file in jsonFiles)
            {
                try
                {
                    var json = File.ReadAllText(file.FullPath);
                    JToken.Parse(json);
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Validation failed for JSON file: {file.FullPath}{Environment.NewLine}{ex.Message}", ex);
                }
            }
        }
    }
}

[TaskName("Build")]
[IsDependentOn(typeof(ValidateJsonTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        for (int i = 0; i < BuildContext.ProjectNames.Length; i++)
        {
            context.DotNetClean($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}.csproj",
                new DotNetCleanSettings
                {
                    Configuration = context.BuildConfiguration
                });


            context.DotNetPublish($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}.csproj",
                new DotNetPublishSettings
                {
                    Configuration = context.BuildConfiguration
                });
        }
    }
}

[TaskName("Package")]
[IsDependentOn(typeof(BuildTask))]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.EnsureDirectoryExists("../../bin/ZipStaging");
        context.CleanDirectory("../../bin/ZipStaging");
        context.EnsureDirectoryExists("../../bin/Releases");
        context.CleanDirectory("../../bin/Releases");
        for (int i = 0; i < BuildContext.ProjectNames.Length; i++)
        {
            context.EnsureDirectoryExists($"../../bin/ZipStaging/{context.Names[i]}");
            context.CopyFiles($"../../bin/{context.BuildConfiguration}/Mods/{BuildContext.ProjectNames[i]}/publish/*", $"../../bin/ZipStaging/{context.Names[i]}");
            context.CopyDirectory($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/assets", $"../../bin/ZipStaging/{context.Names[i]}/assets");
            context.CopyFile($"../../{BuildContext.ProjectNames[i]}/{BuildContext.ProjectNames[i]}/modinfo.json", $"../../bin/ZipStaging/{context.Names[i]}/modinfo.json");
            context.Zip($"../../bin/ZipStaging/{context.Names[i]}", $"../../bin/Releases/{context.Names[i]}_{context.Versions[i]}.zip");
        }
        DeleteDirectorySettings DDS = new DeleteDirectorySettings();
        DDS.Recursive = true;
        context.DeleteDirectory($"../../bin/ZipStaging/", DDS);
        context.DeleteDirectory($"../../bin/Release", DDS);
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(PackageTask))]
public class DefaultTask : FrostingTask
{
}