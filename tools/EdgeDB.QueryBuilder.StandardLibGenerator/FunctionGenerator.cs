using EdgeDB.QueryBuilder.StandardLibGenerator.Models;
using System.Globalization;

namespace EdgeDB.QueryBuilder.StandardLibGenerator
{
    internal class FunctionGenerator
    {
        private const string OUTPUT_PATH = @"C:\Users\lynch\source\repos\EdgeDB\src\EdgeDB.Net.QueryBuilder\Translators\Methods\Generated";
        public static void Generate(IReadOnlyCollection<Function> functions)
        {
            if (!Directory.Exists(OUTPUT_PATH))
                Directory.CreateDirectory(OUTPUT_PATH);

            var grouped = functions.GroupBy(x => x.ReturnType.Name);
            foreach(var item in grouped)
            {
                ProcessGroup(item.Key!, item);
            }
        }

        private static void ProcessGroup(string groupType, IEnumerable<Function> funcs)
        {
            var writer = new CodeWriter();

            using (var namespaceScope = writer.BeginScope("namespace EdgeDB.Translators"))
            using (var classScope = writer.BeginScope($"internal partial class {new CultureInfo("en-US").TextInfo.ToTitleCase(groupType)} : MethodTranslator"))
            {

                foreach (var func in funcs)
                {

                }
            }
        }
    }
}
