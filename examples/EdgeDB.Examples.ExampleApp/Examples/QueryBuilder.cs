using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class QueryBuilder : IExample
    {
        public ILogger? Logger { get; set; }

        public class LinkPerson
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public LinkPerson? BestFriend { get; set; }
        }

        public class Person
        {
            public string? Name { get; set; }
            public string? Email { get; set; }
            public Guid Id { get; set; }
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var collection = client.GetCollection<LinkPerson>();

            var result = await collection.Filter(x => EdgeQL.ILike(x.Name, "j%")).ExecuteAsync();

            var insertResult = collection.Insert(new LinkPerson
            {
                Email = "email@example.com",
                Name = "ExampleName",
                BestFriend = new()
                {
                    Email = "email2@example2.com",
                    Name = "ExampleName2"
                }
            }).UnlessConflictOn(x => x.Email).Build();

            var pretty = Prettify(insertResult.Query);

        }

        public static string Prettify(string queryText)
        {
            // add newlines
            var result = Regex.Replace(queryText, @"({|\(|\)|}|,)", m =>
            {
                switch (m.Groups[1].Value)
                {
                    case "{" or "(" or ",":
                        if (m.Groups[1].Value == "{" && queryText[m.Index + 1] == '}')
                            return m.Groups[1].Value;

                        return $"{m.Groups[1].Value}\n";

                    default:
                        return $"{((m.Groups[1].Value == "}" && (queryText[m.Index - 1] == '{' || queryText[m.Index - 1] == '}')) ? "" : "\n")}{m.Groups[1].Value}{((queryText.Length != m.Index + 1 && (queryText[m.Index + 1] != ',')) ? "\n" : "")}";
                }
            }).Trim().Replace("\n ", "\n");

            // clean up newline func
            result = Regex.Replace(result, "\n\n", m => "\n");

            // add indentation
            result = Regex.Replace(result, "^", m =>
            {
                int indent = 0;

                foreach (var c in result[..m.Index])
                {
                    if (c is '(' or '{')
                        indent++;
                    if (c is ')' or '}')
                        indent--;
                }

                var next = result.Length != m.Index ? result[m.Index] : '\0';

                if (next is '}' or ')')
                    indent--;

                return "".PadLeft(indent * 2);
            }, RegexOptions.Multiline);

            return result;
        }
    }
}
