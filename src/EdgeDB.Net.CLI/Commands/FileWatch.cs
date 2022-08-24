using CommandLine;
using EdgeDB.CLI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Net.CLI.Commands
{
    [Verb("file-watch-internal", Hidden = true)]
    internal class FileWatch : ICommand
    {
        [Option("connection")]
        public string? Connection { get; set; }

        [Option("dir")]
        public string? Dir { get; set; }

        [Option("namespace")]
        public string? Namespace { get; set; }

        private readonly FileSystemWatcher _watcher = new();
        private readonly EdgeDBTcpClient _client;
        public FileWatch()
        {
            if (Connection is null)
                throw new InvalidOperationException("Connection must be specified");

            _client = new(JsonConvert.DeserializeObject<EdgeDBConnection>(Connection)!, new());
        }

        public async Task ExecuteAsync()
        {
            _watcher.Path = Dir;
            _watcher.Filter = "*.edgeql";
            _watcher.IncludeSubdirectories = true;

            _watcher.Error += _watcher_Error;
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Created;
            _watcher.Deleted += _watcher_Deleted;
            _watcher.Renamed += _watcher_Renamed;

            _watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            _watcher.EnableRaisingEvents = true;

            await Task.Delay(-1);
        }

        public async Task GenerateAsync(EdgeQLParser.GenerationTargetInfo info)
        {
            await _client.ConnectAsync();

            var result = await EdgeQLParser.ParseAndGenerateAsync(_client, Namespace!, info);

            File.WriteAllText(info.TargetFilePath!, result.Code);
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            var info = EdgeQLParser.GetTargetInfo(e.FullPath, Dir!);

            if (info.GeneratedTargetExistsAndIsUpToDate())
                return;

            Task.Run(async () => await GenerateAsync(info));
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            Console.Error.WriteLine($"An error occored: {e.GetException()}");
        }
    }
}
