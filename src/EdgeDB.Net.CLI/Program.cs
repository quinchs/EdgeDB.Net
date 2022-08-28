using CommandLine;
using CommandLine.Text;
using EdgeDB.CLI;
using EdgeDB.CLI.Arguments;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var commands = typeof(Program).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

var parser = new Parser(x =>
{
    x.HelpWriter = null;
});

var result = parser.ParseArguments(args, commands.ToArray());

try
{
    var commandResult = await result.WithParsedAsync<ICommand>(x =>
    {
        if(x is LogArgs logArgs)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logArgs.LogLevel)
                .WriteTo.Console()
                .CreateLogger();
        }

        return x.ExecuteAsync(Log.Logger);
    });


    result.WithNotParsed(err =>
    {
        var helpText = HelpText.AutoBuild(commandResult, h =>
        {
            h.AdditionalNewLineAfterOption = true;
            h.Heading = "EdgeDB.Net CLI";
            h.Copyright = "Copyright (c) 2022 EdgeDB";

            return h;
        }, e => e, verbsIndex: true);

        Console.WriteLine(helpText);
    });

}
catch (Exception x)
{
    Console.WriteLine(x);   
}



