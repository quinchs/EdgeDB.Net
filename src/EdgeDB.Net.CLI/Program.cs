// load commands
using CommandLine;
using CommandLine.Text;
using EdgeDB.CLI;

var commands = typeof(Program).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x == typeof(ICommand)));

var parser = new Parser(x =>
{
    x.HelpWriter = null;
});

var result = parser.ParseArguments(args, commands.ToArray());

var commandResult = await result.WithParsedAsync<ICommand>(x => x.ExecuteAsync());

var helpText = HelpText.AutoBuild(commandResult, h =>
{
    h.AdditionalNewLineAfterOption = true;
    h.Heading = "EdgeDB.Net CLI";
    h.Copyright = "Copyright (c) 2022 EdgeDB";

    return h;
}, e => e, verbsIndex: true);

Console.WriteLine(helpText);
