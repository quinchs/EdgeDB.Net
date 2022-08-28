using Serilog;

namespace EdgeDB.CLI;

interface ICommand
{
    Task ExecuteAsync(ILogger logger);
}