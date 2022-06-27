using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema
{   
    internal class SchemaIntrospector
    {
        private static readonly ConcurrentDictionary<IEdgeDBQueryable, SchemaInfo> _schemas;

        static SchemaIntrospector()
        {
            _schemas = new ConcurrentDictionary<IEdgeDBQueryable, SchemaInfo>();
        }

        public static ValueTask<SchemaInfo> GetOrCreateSchemaIntrospectionAsync(IEdgeDBQueryable edgedb, CancellationToken token = default)
        {
            if (_schemas.TryGetValue(edgedb, out var info))
                return ValueTask.FromResult(info);
            return new ValueTask<SchemaInfo>(IntrospectSchemaAsync(edgedb, token));
        }

        private static async Task<SchemaInfo> IntrospectSchemaAsync(IEdgeDBQueryable edgedb, CancellationToken token)
            => _schemas[edgedb] = new SchemaInfo(await edgedb.QueryAsync<ObjectType>(Properties.Resources.INTROSPECT_QUERY, token: token).ConfigureAwait(false));
    }
}
