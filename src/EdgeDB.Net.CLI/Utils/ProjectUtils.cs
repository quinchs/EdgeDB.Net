using CliWrap;
using System;
using System.Collections.Generic;
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

        public static async Task CreateGeneratedProjectAsync(string root, string name)
        {
            var result = await Cli.Wrap("dotnet")
                .WithArguments($"new classlib --framework \"net6.0\" -n {name}")
                .WithWorkingDirectory(Directory.GetParent(root)?.FullName ?? root)
                .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
                .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
                .ExecuteAsync();

            if (result.ExitCode != 0)
                throw new IOException($"Failed to create new project");

            result = await Cli.Wrap("dotnet")
                .WithArguments("add package EdgeDB.Net.Driver")
                .WithWorkingDirectory(Path.Combine(Directory.GetParent(root)?.FullName ?? root, name))
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
