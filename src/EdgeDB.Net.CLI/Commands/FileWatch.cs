using CommandLine;
using EdgeDB.CLI;
using EdgeDB.CLI.Utils;
using EdgeDB.Net.CLI.Utils;
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
        private EdgeDBTcpClient? _client;
        private readonly SemaphoreSlim _mutex = new(1, 1);


        public async Task ExecuteAsync()
        {
            if (Connection is null)
                throw new InvalidOperationException("Connection must be specified");

            _client = new(JsonConvert.DeserializeObject<EdgeDBConnection>(Connection)!, new());

            _watcher.Path = Dir!;
            _watcher.Filter = "*.edgeql";
            _watcher.IncludeSubdirectories = true;

            _watcher.Error += _watcher_Error;
            _watcher.Changed += CreatedAndUpdated;
            _watcher.Created += CreatedAndUpdated;
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

            ProjectUtils.RegisterProcessAsWatcher(Dir!);

            await Task.Delay(-1);
        }

        public async Task GenerateAsync(EdgeQLParser.GenerationTargetInfo info)
        {
            await _mutex.WaitAsync().ConfigureAwait(false);

            try
            {
                await _client!.ConnectAsync();

                try
                {
                    var result = await EdgeQLParser.ParseAndGenerateAsync(_client, Namespace!, info);
                    File.WriteAllText(info.TargetFilePath!, result.Code);
                }
                catch (EdgeDBErrorException err)
                {
                    // error with file
                    Console.WriteLine(err.Message);
                }
            }
            catch(Exception x)
            {
                Console.WriteLine(x);
            }
            finally
            {
                _mutex.Release();
            }
        }

        private void CreatedAndUpdated(object sender, FileSystemEventArgs e)
        {
            FileUtils.WaitForHotFile(e.FullPath);

            var info = EdgeQLParser.GetTargetInfo(e.FullPath, Dir!);

            if (info.GeneratedTargetExistsAndIsUpToDate())
                return;

            Task.Run(async () =>
            {
                await Task.Delay(200);
                await GenerateAsync(info);
            });
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var info = EdgeQLParser.GetTargetInfo(e.FullPath, Dir!);

            if (File.Exists(info.TargetFilePath))
                File.Delete(info.TargetFilePath);
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            var info = EdgeQLParser.GetTargetInfo(e.OldFullPath, Dir!);

            if (File.Exists(info.TargetFilePath))
            {
                var newInfo = EdgeQLParser.GetTargetInfo(e.FullPath, Dir!);
                File.Move(info.TargetFilePath, newInfo.TargetFilePath!);
            }
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            Console.Error.WriteLine($"An error occored: {e.GetException()}");
        }
    }
}
