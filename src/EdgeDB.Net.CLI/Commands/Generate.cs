using CommandLine;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
using EdgeDB.Codecs;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EdgeDB.CLI;

[Verb("generate", HelpText = "Generate or updates csharp classes from .edgeql files.")]
public class Generate : ConnectionArguments, ICommand
{
    [Option('p', "build-project", HelpText = "Whether or not to create the default class library that will contain the generated source code.")]
    public bool GenerateProject { get; set; } = true;

    [Option('o', "output", HelpText = "The output directory for the generated source to be placed.")]
    public string? OutputDirectory { get; set; }

    [Option('n', "generated-project-name", HelpText = "The name of the generated project.")]
    public string GeneratedProjectName { get; set; } = "EdgeDB.Generated";

    [Option('f', "force", HelpText = "Force regeneration of files")]
    public bool Force { get; set; }
    
    public async Task ExecuteAsync()
    {
        // get connection info
        var connection = GetConnection();

        // create the client
        var client = new EdgeDBTcpClient(connection, new());

        Console.WriteLine($"Connecting to {connection.Hostname}:{connection.Port}...");
        await client.ConnectAsync();

        var projectRoot = ProjectUtils.GetProjectRoot();

        OutputDirectory ??= Path.Combine(projectRoot, GeneratedProjectName);

        if (GenerateProject && !Directory.Exists(OutputDirectory))
        {
            Console.WriteLine($"Creating project {GeneratedProjectName}...");
            await ProjectUtils.CreateGeneratedProjectAsync(projectRoot, GeneratedProjectName);
        }

        // find edgeql files
        var edgeqlFiles = ProjectUtils.GetTargetEdgeQLFiles(projectRoot).ToArray();

        // compute the hashes for each file
        string[] hashs = edgeqlFiles.Select(x => HashUtils.HashEdgeQL(File.ReadAllText(x))).ToArray();

        Console.WriteLine($"Generating {edgeqlFiles.Length} files...");

        for(int i = 0; i != edgeqlFiles.Length; i++)
        {
            var file = edgeqlFiles[i];
            var hash = hashs[i];
            var targetFileName = TextUtils.ToPascalCase(Path.GetFileName(file).Split('.')[0]);
            var targetOutputPath = Path.Combine(projectRoot, GeneratedProjectName, $"{targetFileName}.g.cs");
            var edgeql = File.ReadAllText(file);

            if (!Force && File.Exists(targetOutputPath))
            {
                // check the hashes 
                var hashHeader = Regex.Match(File.ReadAllLines(targetOutputPath)[1], @"\/\/ edgeql:([0-9a-fA-F]{64})");

                if(hashHeader.Success && hashHeader.Groups[1].Value == hash)
                {
                    Console.WriteLine($"{i + 1}: Skipping {file}: File already generated");
                    continue;
                }
            }

            try
            {
                var result = await EdgeQLParser.ParseAndGenerateAsync(client, edgeql, GeneratedProjectName, hash, targetFileName);
                File.WriteAllText(targetOutputPath, result.Code);
            }
            catch (EdgeDBErrorException error)
            {
                Console.WriteLine($"Failed to parse {file} (line {error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65523).ToString() ?? "??"}, column {error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65524).ToString() ?? "??"}):");
                Console.WriteLine(error.Message);
                Console.WriteLine(QueryErrorFormatter.FormatError(edgeql, error));
                Console.WriteLine($"{i + 1}: Skipping {file}: File contains errors");
                continue;
            }

            Console.WriteLine($"{i + 1}: {file} => {targetOutputPath}");
        }

        Console.WriteLine("Generation complete!");
    }
}