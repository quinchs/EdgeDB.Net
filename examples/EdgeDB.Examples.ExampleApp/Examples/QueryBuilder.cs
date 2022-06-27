using EdgeDB.Schema;
using EdgeDB.Serializer;
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
                .UnlessConflict()
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
                .UnlessConflict()
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
                .UnlessConflict()
                .Else(q => 
                    q.Update(old => new LinkPerson
                    { 
                        Name = old!.Name!.ToUpper()
                    })
                ).Build().Prettify();
        }
    }
}
