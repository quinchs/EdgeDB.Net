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
        {
            var result = await QueryBuilder.Select<ObjectType>(ctx => new ObjectType
            {
                Id = ctx.Include<Guid>(),
                IsAbstract = ctx.Include<bool>(),
                Name = ctx.Include<string>(),
                Properties = ctx.IncludeMultiLink(() => new Property
                {
                    Cardinality = (string)ctx.UnsafeLocal<object>("cardinality") == "One"
                        ? ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.One : DataTypes.Cardinality.AtMostOne
                        : ctx.UnsafeLocal<bool>("required") ? DataTypes.Cardinality.AtLeastOne : DataTypes.Cardinality.Many,
                    Name = ctx.Include<string>(),
                    TargetId = ctx.UnsafeLocal<Guid>("target.id"),
                    IsLink = ctx.Raw<object>("[IS schema::Link]") != null,
                    IsExclusive = ctx.Raw<bool>("exists (select .constraints filter .name = 'std::exclusive')"),
                    IsComputed = EdgeQL.Length(ctx.UnsafeLocal<object>("computed_fields")) != 0,
                    IsReadonly = ctx.UnsafeLocal<bool>("readonly"),
                    HasDefault = ctx.Raw<bool>("EXISTS .default or (\"std::sequence\" in .target[IS schema::ScalarType].ancestors.name)")

                })
            }).Filter((x, ctx) => !ctx.UnsafeLocal<bool>("builtin")).ExecuteAsync(edgedb, token);
            
            return _schemas[edgedb] = new SchemaInfo(result);
        }
    }
}
