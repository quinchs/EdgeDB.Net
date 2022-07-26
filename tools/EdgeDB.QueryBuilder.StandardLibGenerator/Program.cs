using EdgeDB;
using EdgeDB.QueryBuilder.StandardLibGenerator;
using EdgeDB.QueryBuilder.StandardLibGenerator.Models;

var edgedb = new EdgeDBClient();

//var operators = await QueryBuilder.Select<Operator>().Filter(x => !x.IsAbstract).ExecuteAsync(edgedb);
var functions = await QueryBuilder.Select<Function>().Filter(x => x.BuiltIn).ExecuteAsync(edgedb)!;

FunctionGenerator.Generate(functions);

await Task.Delay(-1);