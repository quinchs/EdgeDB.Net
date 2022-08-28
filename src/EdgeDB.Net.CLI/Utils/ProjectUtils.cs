using CliWrap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Utils
{   
    internal class ProjectUtils
    {
        public static string GetProjectRoot()
        {
            var directory = Environment.CurrentDirectory;
            bool foundRoot = false;

            while (!foundRoot)
            {
                if (
                    !(foundRoot = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly).Any(x => x.EndsWith($"{Path.DirectorySeparatorChar}edgedb.toml"))) && 
                    (directory = Directory.GetParent(directory!)?.FullName) is null)
                    throw new FileNotFoundException("Could not find edgedb.toml in the current and parent directories");
            }

            return directory;
        }

        public static int StartWatchProcess(EdgeDBConnection connection, string root, string outputDir, string @namespace)
        {
            var current = Process.GetCurrentProcess();
            var connString = JsonConvert.SerializeObject(connection).Replace("\"", "\\\"");

            return Process.Start(new ProcessStartInfo
            {
                FileName = current.MainModule!.FileName,
                Arguments = $"file-watch-internal --connection \"{connString}\" --dir {root} --output \"{outputDir}\" --namespace \"{@namespace}\"",
                UseShellExecute = true,
            })!.Id;
        }

        public static Process? GetWatcherProcess(string root)
        {
            var file = Path.Combine(root, "edgeql.dotnet.watcher.process");
            if (File.Exists(file) && int.TryParse(File.ReadAllText(file), out var id))
            {
                try
                {
                    return Process.GetProcesses().FirstOrDefault(x => x.Id == id);
                }
                catch { return null; }
            }

            return null;
        }

        public static void RegisterProcessAsWatcher(string root)
        {
            var id = Process.GetCurrentProcess().Id;

            File.WriteAllText(Path.Combine(root, "edgeql.dotnet.watcher.process"), $"{id}");

            // add to gitignore if its here
            var gitignore = Path.Combine(root, ".gitignore");
            if (File.Exists(gitignore))
            {
                var contents = File.ReadAllText(gitignore);
                
                if(!contents.Contains("edgeql.dotnet.watcher.process"))
                {
                    contents += $"{Environment.NewLine}# EdgeDB.Net CLI watcher info file{Environment.NewLine}edgeql.dotnet.watcher.process";
                    File.WriteAllText(gitignore, contents);
                }
            }
        }

        public static async Task CreateGeneratedProjectAsync(string root, string name)
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments($"new classlib --framework \"net6.0\" -n {name}")
                .WithWorkingDirectory(root)
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync();

            if (result.ExitCode != 0)
                throw new IOException($"Failed to create new project");

            result = await Cli.Wrap("dotnet")
                .WithArguments("add package EdgeDB.Net.Driver")
                .WithWorkingDirectory(Path.Combine(root, name))
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync();

            if (result.ExitCode != 0)
                throw new IOException($"Failed to create new project");

            // remove default file
            File.Delete(Path.Combine(root, name, "Class1.cs"));
        }

        public static IEnumerable<string> GetTargetEdgeQLFiles(string root)
            => Directory.GetFiles(root, "*.edgeql", SearchOption.AllDirectories).Where(x => !x.StartsWith(Path.Combine(root, "dbschema", "migrations")));
    }
}
