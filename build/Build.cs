using System;
using System.IO;
using System.Linq;
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
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    readonly Guid PublishProjectId = new Guid("05f5ac6f-e1c7-40e7-b0b4-ce81b7c42a47");

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            
            var b = GitRepository.Branch;
            var ps = Solution.Projects;


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

            DotNetPublish(s => s
                .SetProject(p)
                .SetConfiguration(Configuration)
                .SetOutput(OutputDirectory)
                .SetVersion("1.1.0.0")
                .SetNoRestore(true));
        });

}
