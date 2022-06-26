using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            var builder = new QueryBuilder<LinkPerson>();

            // get or create john
            var john = await builder
                .Insert(new LinkPerson { Email = "johndoe@email.com", Name = "John Doe" })
                .UnlessConflictOn(x => x.Email)
                .ElseReturn()
                .ExecuteAsync(client);

            // get or create jane
            var jane = await builder
                .With("john", john)
                .Insert(ctx => new LinkPerson 
                { 
                    Name = "Jane Doe", 
                    Email = "janedoe@email.com", 
                    BestFriend = ctx.Global<LinkPerson>("john") 
                })
                .UnlessConflictOn(x => x.Email)
                .ElseReturn()
                .ExecuteAsync(client);

            Logger!.LogInformation("John result: {@john}", john);
            Logger!.LogInformation("Jane result: {@jane}", jane);

            // anon types
            var anonPerson = await builder
                .Select(ctx => new
                {
                    Name = ctx.Include<string>(),
                    Email = ctx.Include<string>(),
                    HasFriend = ctx.Local<LinkPerson>("BestFriend") != null
                })
                .ExecuteAsync(client);

            var test = builder
                .Insert(new LinkPerson()
                {
                    Name = "upsert demo",
                    Email = "upsert@mail.com"
                })
                .UnlessConflictOn(x => x.Email)
                .Else(q => 
                    q.Update(old => new LinkPerson
                    { 
                        Name = old.Name!.ToUpper()
                    })
                );
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
