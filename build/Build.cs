using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
[NotifyFailure]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    readonly Guid PublishProjectId = new Guid("05f5ac6f-e1c7-40e7-b0b4-ce81b7c42a47");

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Triggers(NotifyOfResult)
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            Serilog.Log.Information("Restoring");
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        }).DependsOn(Clean);

    Target Test => _ => _
        .After(Restore)
        .Executes(() =>
        {
            Serilog.Log.Information("Testing");

            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetNoRestore(true));

        });

    Target Publish => _ => _
        .DependsOn(Restore)
        .DependsOn(Test)
        .Executes(() =>
        {
            Serilog.Log.Information("Compiling");

            var p = Solution.GetProject(PublishProjectId);

            Serilog.Log.Information("Identified project as {Project}", p.Name);

            Serilog.Log.Information("Publishing with {Configuration}", Configuration);

            DotNetPublish(s => s
                .SetProject(p)
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory)
                .SetVersion("1.1.0.0")
                .SetNoRestore(true));
        });


    Target NotifyOfResult => _ => _
        .AssuredAfterFailure() //always run        
        .DependsOn(Publish)
        .Executes(async () =>
        {

            await Task.Delay(1000);
            Serilog.Log.Fatal("TARGET Oh noes! The build failed!");
            await Task.Delay(1000);
            Serilog.Log.Debug("Sample message");                       

        });
}
