using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ExampleApp.Examples
{
    internal class Globals : IExample
    {
        public ILogger? Logger { get; set; }

        public async Task ExecuteAsync(EdgeDBClient client)
        {
            await using var clientInstance = await client.GetOrCreateClientAsync();
            clientInstance.WithGlobals(new Dictionary<string, object?>
            {
                {"test", "Hello!" }
            });

            var result = await clientInstance.QuerySingleAsync<string>("select global test");
        }
    }
}
