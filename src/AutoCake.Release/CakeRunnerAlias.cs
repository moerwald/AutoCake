using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Common.Tools.Cake;
using Cake.Core;

public static class CakeRunnerAlias
{
    /// <summary>
    /// Proxy method to <see cref="RunCake(ICakeContext, string, CakeSettings)"/>
    /// </summary>
    /// <param name="context"></param>
    /// <param name="settings"></param>
    public static void RunCake(ICakeContext context, CakeSettings settings = null) => RunCake(context, null, settings); 


    /// <summary>
    /// Parses command line arguments and invokes cakes runner object.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="script"></param>
    /// <param name="settings"></param>
    public static void RunCake(ICakeContext context, string script, CakeSettings settings = null)
    {
        var rawArgs = QuoteAwareStringSplitter.Split(GetCommandLine())
                                              .Skip(1) // Skip executable.
                                              .ToArray();

        settings = settings ?? new CakeSettings();

        var ar = new InternalArgumentParser(context.Log);
        var baseOptions = ar.Parse(rawArgs);
        var mergedOptions = new Dictionary<string, string>();

        // Merge basedOptions and settings.Arguments to mergedOptions 
        foreach (var optionsArgument in baseOptions)
        {
            if (optionsArgument.Key == "target" || string.IsNullOrEmpty(optionsArgument.Value))
            {
                continue;
            }

            mergedOptions[optionsArgument.Key] = optionsArgument.Value;
        }

        if (settings.Arguments != null)
        {
            foreach (var optionsArgument in settings.Arguments)
            {
                if (string.IsNullOrEmpty(optionsArgument.Value))
                {
                    mergedOptions.Remove(optionsArgument.Key);
                    continue;
                }

                mergedOptions[optionsArgument.Key] = optionsArgument.Value;
            }
        }

        mergedOptions.Remove("script");
        settings.Arguments = mergedOptions;
        script = script ?? "./build.cake";

        // Invoke cake runner
        var cakeRunner = new FixedCakeRunner(
            context.FileSystem,
            context.Environment,
            context.Globber,
            context.ProcessRunner,
            context.Tools,
            context.Log);
        cakeRunner.ExecuteScript(script, settings);

        // Local functions
        string GetCommandLine()
        {
            var envType = typeof(Environment);
            var propertyInfo = envType.GetProperty("CommandLine");
            return propertyInfo != null 
                ? (string) propertyInfo.GetMethod.Invoke(null, new object[0])
                : string.Join(" ", Environment.GetCommandLineArgs());
        }
    }
}