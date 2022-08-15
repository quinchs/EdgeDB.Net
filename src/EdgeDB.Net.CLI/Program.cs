// load commands
using CommandLine;
using EdgeDB.CLI;

var commands = typeof(Program).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

await Parser.Default.ParseArguments(args, commands.ToArray())
    .WithNotParsed(HandleNoCommand)
    .WithParsedAsync<ICommand>(x => x.ExecuteAsync);


void HandleNoCommand(IEnumerable<Error> errors)
{
    
}    
