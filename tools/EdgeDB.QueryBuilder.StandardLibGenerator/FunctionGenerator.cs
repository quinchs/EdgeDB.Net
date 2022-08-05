using EdgeDB.DataTypes;
using EdgeDB.StandardLibGenerator.Models;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace EdgeDB.StandardLibGenerator
{
    internal class FunctionGenerator
    {
        private const string STDLIB_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\stdlib";
        private const string OUTPUT_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Translators\Methods\Generated";
        private static readonly TextInfo _textInfo = new CultureInfo("en-US").TextInfo;
        private static readonly List<TypeNode> _generatedTypes = new();
        private static readonly Regex _groupRegex = new(@"(.+?)<.+?>");
        private static CodeWriter? _edgeqlClassWriter;
        private static EdgeDBClient? _client;

        public static async ValueTask GenerateAsync(CodeWriter eqlWriter, EdgeDBClient client, IReadOnlyCollection<Function> functions)
        {
            _client = client;
            _edgeqlClassWriter = eqlWriter;
            if (!Directory.Exists(OUTPUT_PATH))
                Directory.CreateDirectory(OUTPUT_PATH);

            try
            {
                var grouped = functions.GroupBy(x =>
                {
                    var m = _groupRegex.Match(x.ReturnType!.Name!);
                    return m.Success ? m.Groups[1].Value : x.ReturnType.Name;

                });
                foreach (var item in grouped)
                {
                    await ProcessGroup(item.Key!, item);
                }
            }
            catch(Exception x)
            {

            }
        }

        private static async ValueTask ProcessGroup(string groupType, IEnumerable<Function> funcs)
        {
            var writer = new CodeWriter();

            var edgedbType = funcs.FirstOrDefault(x => x.ReturnType!.Name! == groupType)?.ReturnType!;
            var translatorType = TypeUtils.TryGetType(groupType, out var tInfo) ? await BuildType(tInfo, edgedbType, TypeModifier.SingletonType, true) : groupType switch
            {
                "tuple" => typeof(ITuple).Name,
                "array" => typeof(Array).Name,
                "set" => typeof(IEnumerable).Name,
                "range" => "IRange",
                _  => groupType.Contains("::") ? await BuildType(new(groupType, null), edgedbType, TypeModifier.SingletonType, true) : throw new Exception($"Failed to find matching type for {groupType}")
            };

            writer.AppendLine("using EdgeDB;");
            writer.AppendLine("using EdgeDB.DataTypes;");
            writer.AppendLine("using System.Runtime.CompilerServices;");
            writer.AppendLine();

            using (var namespaceScope = writer.BeginScope("namespace EdgeDB.Translators"))
            using (var classScope = writer.BeginScope($"internal partial class {_textInfo.ToTitleCase(groupType.Replace("::", " ")).Replace(" ", "")} : MethodTranslator<{translatorType}>"))
            {
                foreach (var func in funcs)
                {
                    try
                    {
                        var funcName = _textInfo.ToTitleCase(func.Name!.Split("::")[1].Replace("_", " ")).Replace(" ", "");

                        if (!TypeUtils.TryGetType(func.ReturnType!.Name!, out var returnTypeInfo))
                            throw new Exception($"Faield to get type {groupType}");

                        var dotnetReturnType = await BuildType(returnTypeInfo, func.ReturnType, TypeModifier.SingletonType);

                        switch (func.ReturnTypeModifier)
                        {
                            case TypeModifier.OptionalType:
                                dotnetReturnType += "?";
                                break;
                            case TypeModifier.SetOfType:
                                dotnetReturnType = $"IEnumerable<{dotnetReturnType}>";
                                break;
                            default:
                                break;
                        }

                        var parameters = func.Parameters!.Select<Parameter, (Parameter Parameter, TypeNode Node)?>(x =>
                        {
                            if (!TypeUtils.TryGetType(x.Type!.Name!, out var info))
                                return null;

                            return (x, info);
                        });

                        if (parameters.Any(x => !x.HasValue))
                            throw new Exception("No parameter matches found");

                        string[] parsedParameters = new string[parameters.Count()];

                        for(int i = 0; i != parsedParameters.Length; i++)
                        {
                            var x = parameters.ElementAt(i);
                            var type = BuildType(x!.Value.Node, x.Value.Parameter!.Type!, x.Value.Parameter.TypeModifier, true);
                            var name = x.Value.Parameter.Name;
                            string @default = "";
                            if (x.Value.Parameter.Default is not null)
                                @default = x.Value.Parameter.Default == "{}" ? "null" : await ParseDefaultAsync(x.Value.Parameter.Default);

                            parsedParameters[i] = $"{type} {name}{(!string.IsNullOrEmpty(@default) ? $" = {@default}" : "")}";
                        }

                        var strongMappedParameters = string.Join(", ", parsedParameters);
                        var parsedMappedParameters = string.Join(", ", parameters.Select(x => $"string? {x!.Value.Parameter.Name}Param"));

                        writer.AppendLine($"[MethodName(EdgeQL.{funcName})]");
                        writer.AppendLine($"public string {funcName}({parsedMappedParameters})");

                        using(var methodScope = writer.BeginScope())
                        {
                            var methodBody = $"return $\"{func.Name}(";

                            string[] parsedParams = new string[func.Parameters.Length];

                            for(int i = 0; i != parsedParams.Length; i++)
                            {
                                var param = func.Parameters[i];

                                var value = "";
                                if (param.TypeModifier != TypeModifier.OptionalType)
                                    value = $"{{{param.Name}Param}}";
                                else
                                    value = $"{{({param.Name}Param is not null ? \"{param.Name}Param, \" : \"\")}}";

                                if (param.Kind == ParameterKind.NamedOnlyParam)
                                    value = $"{param.Name} := {{{param.Name}Param}}";

                                parsedParams[i] = value;
                            }

                            methodBody += string.Join(", ", parsedParams) + ")\";";

                            writer.AppendLine(methodBody);
                        }
                        writer.AppendLine();

                        _edgeqlClassWriter!.AppendLine($"public static {dotnetReturnType} {funcName}({strongMappedParameters})");
                        _edgeqlClassWriter.AppendLine("    => default!");
                    }
                    catch(Exception x)
                    {
                        Console.WriteLine(x);
                    }
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(OUTPUT_PATH, $"{_textInfo.ToTitleCase(groupType).Replace(":", "")}.g.cs"), writer.ToString());
            }
            catch(Exception x)
            {

            }
        }

        private static async ValueTask<string> BuildType(TypeNode node, EdgeDB.StandardLibGenerator.Models.Type edgedbType, TypeModifier modifier, bool shouldGenerate = true)
        {
            var name = node.IsGeneric
                            ? "object"
                            : node.DotnetType is null && node.RequiresGeneration && shouldGenerate
                                ? await GenerateType(node, edgedbType)
                                : node.DotnetType?.Name ?? "object";

            return modifier switch
            {
                TypeModifier.OptionalType => $"{name}?",
                TypeModifier.SingletonType => name,
                TypeModifier.SetOfType => $"IEnumerable<{name}>",
                _ => name
            };
        }

        private static async Task<string> GenerateType(TypeNode node, EdgeDB.StandardLibGenerator.Models.Type edgedbType)
        {
            var meta = await edgedbType.GetMetaInfoAsync(_client!);

            switch (meta.Type)
            {
                case MetaInfoType.Object:
                    {

                    }
                    break;
                case MetaInfoType.Enum:
                    {

                    }
                    break;
                default:
                    throw new Exception($"Unknown stdlib builder for type {edgedbType.TypeOfSelf}");
            }

            return "";
        }


        private static readonly Regex _typeCastOne = new(@"(<[^<]*?>)");
        private static async Task<string> ParseDefaultAsync(string @default)
        {
            var result = await _client!.QuerySingleAsync<object>($"select {@default}");
            return result?.ToString() ?? "null";
        }
    }
}
