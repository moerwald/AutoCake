using System.Collections.Generic;
using System.Globalization;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Core.Tooling;

public class GitTool : Tool<GitToolSettings>
{
    readonly ICakeLog logger;

    public GitTool(IFileSystem fileSystem,
                   ICakeEnvironment environment,
                   IProcessRunner processRunner,
                   IToolLocator tools,
                   ICakeLog logger)
        : base(fileSystem, environment, processRunner, tools)
    {
        this.logger = logger;
    }

    public GitTool(ICakeContext context)
        : this(context.FileSystem,
               context.Environment,
               context.ProcessRunner,
               context.Tools,
               context.Log)
    {
    }

    public int LastExitCode { get; set; }

    public bool AllowExitCodeForStatus { get; set; }

    public void RunGit(GitToolSettings settings, ProcessArgumentBuilder args)
    {
        Run(settings, args);
        if (LastExitCode != 0)
        {
            const string message = "{0}: Process returned an error (exit code {1}).";
            throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, GetToolName(), LastExitCode));
        }
    }

    public int RunGitCheck(GitToolSettings settings, ProcessArgumentBuilder args)
    {
        LastExitCode = 0;
        Run(settings, args);
        return LastExitCode;
    }

    /// <summary>
    /// Called by Tool base class
    /// </summary>
    /// <returns></returns>
    protected override string GetToolName() => "Git"; 

    /// <summary>
    /// Called by Tool base class to retrieve the executables that should be invoked with the given arguments
    /// when calling <see cref="Run"/>.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<string> GetToolExecutableNames() => new List<string> {"Git.exe", "git.exe"};

    /// <summary>
    /// Called by Tool base class the inject the exit code of the executable retrieved via <see cref="GetToolExecutableNames"/>.
    /// </summary>
    /// <param name="exitCode"></param>
    protected override void ProcessExitCode(int exitCode) => LastExitCode = exitCode; 

}