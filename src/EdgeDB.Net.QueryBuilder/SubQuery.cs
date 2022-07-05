using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class SubQuery
    {
        public string? Query { get; init; }
        public bool RequiresIntrospection { get; init; }
        public Func<SchemaInfo, string>? Builder { get; init; }

        public SubQuery(Func<SchemaInfo, string> builder)
        {
            RequiresIntrospection = true;
            Builder = builder;
        }

        public SubQuery(string query)
        {
            Query = query;
        }

        public SubQuery Build(SchemaInfo info)
        {
            return new SubQuery(Builder!(info));
        }
    }
}
