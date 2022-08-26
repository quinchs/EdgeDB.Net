using CommandLine;
using EdgeDB.CLI.Arguments;
using EdgeDB.CLI.Utils;
using EdgeDB.Codecs;
using Newtonsoft.Json;
using System.Diagnostics;
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

    [Option('n', "project-name", HelpText = "The name of the generated project.")]
    public string GeneratedProjectName { get; set; } = "EdgeDB.Generated";

    [Option('f', "force", HelpText = "Force regeneration of files")]
    public bool Force { get; set; }

    [Option("watch", HelpText = "Listens for any changes or new edgeql files and generates them automatically")]
    public bool Watch { get; set; }

    public async Task ExecuteAsync()
    {
        // get connection info
        var connection = GetConnection();

        // create the client
        var client = new EdgeDBTcpClient(connection, new());

        Console.WriteLine($"Connecting to {connection.Hostname}:{connection.Port}...");
        await client.ConnectAsync();

        var projectRoot = ProjectUtils.GetProjectRoot();

        OutputDirectory ??= projectRoot;

        Directory.CreateDirectory(OutputDirectory);

        if (GenerateProject && !Directory.Exists(Path.Combine(OutputDirectory, GeneratedProjectName)))
        {
            Console.WriteLine($"Creating project {GeneratedProjectName}...");
            await ProjectUtils.CreateGeneratedProjectAsync(OutputDirectory, GeneratedProjectName);
        }
        
        if(GenerateProject)
            OutputDirectory = Path.Combine(OutputDirectory, GeneratedProjectName);

        // find edgeql files
        var edgeqlFiles = ProjectUtils.GetTargetEdgeQLFiles(projectRoot).ToArray();
        
        Console.WriteLine($"Generating {edgeqlFiles.Length} files...");

        for(int i = 0; i != edgeqlFiles.Length; i++)
        {
            var file = edgeqlFiles[i];
            var info = EdgeQLParser.GetTargetInfo(file, OutputDirectory);

            if (!Force && info.GeneratedTargetExistsAndIsUpToDate())
            {
                Console.WriteLine($"{i + 1}: Skipping {file}: File already generated and up-to-date.");
                continue;
            }

            try
            {
                var result = await EdgeQLParser.ParseAndGenerateAsync(client, GeneratedProjectName, info);
                File.WriteAllText(info.TargetFilePath!, result.Code);
            }
            catch (EdgeDBErrorException error)
            {
                Console.WriteLine($"Failed to parse {file} (line {error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65523).ToString() ?? "??"}, column {error.ErrorResponse.Attributes.FirstOrDefault(x => x.Code == 65524).ToString() ?? "??"}):");
                Console.WriteLine(error.Message);
                Console.WriteLine(QueryErrorFormatter.FormatError(info.EdgeQL!, error));
                Console.WriteLine($"{i + 1}: Skipping {file}: File contains errors");
                continue;
            }

            Console.WriteLine($"{i + 1}: {file} => {info.TargetFilePath}");
        }

        Console.WriteLine("Generation complete!");

        if(Watch)
        {
            var existing = ProjectUtils.GetWatcherProcess(projectRoot);

            if(existing is not null)
            {
                Console.WriteLine("Watching already running");
                return;
            }

            StartWatchProcess(connection);
        }
    }

    public void StartWatchProcess(EdgeDBConnection connection)
    {
        var current = Process.GetCurrentProcess();
        var connString = JsonConvert.SerializeObject(connection).Replace("\"", "\\\"");

        Process.Start(new ProcessStartInfo
        {
            FileName = current.MainModule.FileName,
            Arguments = $"file-watch-internal --connection \"{connString}\" --dir \"{OutputDirectory}\" --namespace \"{GeneratedProjectName}\"",
            UseShellExecute = true,
        });


        

        //process.StartInfo.Arguments = 
        //process.StartInfo.UseShellExecute = true;
        //process.Start();
    }
}