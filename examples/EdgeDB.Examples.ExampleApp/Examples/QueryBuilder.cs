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
            var collection = client.GetCollection<LinkPerson>();

            // To be written
        }
    }
}
