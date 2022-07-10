using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.Schema;
using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryUtils
    {
        private const string VARIABLE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random _rng = new();

        public static bool IsLink(Type type, out bool isMultiLink, [MaybeNullWhen(false)]out Type? innerLinkType)
        {
            innerLinkType = null;
            isMultiLink = false;

            Type? enumerableType = null;
            if (type != typeof(string) && (enumerableType = type.GetInterfaces().FirstOrDefault(x => ReflectionUtils.IsInstanceOfGenericType(typeof(IEnumerable<>), x))) != null)
            {
                innerLinkType = enumerableType.GenericTypeArguments[0];
                isMultiLink = true;
                return true;
            }

            return TypeBuilder.IsValidObjectType(type);
        }

        internal static string ParseObject(object? obj)
        {
            if (obj is null)
                return "{}";

            if (obj is Enum enm)
            {
                var type = enm.GetType();
                var att = type.GetCustomAttribute<EnumSerializerAttribute>();
                return att != null ? att.Method switch
                {
                    SerializationMethod.Lower => $"\"{obj.ToString()?.ToLower()}\"",
                    SerializationMethod.Numeric => Convert.ChangeType(obj, type.BaseType ?? typeof(int)).ToString() ?? "{}",
                    _ => "{}"
                } : $"\"{obj}\"";
            }

            return obj switch
            {
                SubQuery query when !query.RequiresIntrospection => query.Query!,
                string str => $"\"{str}\"",
                char chr => $"\"{chr}\"",
                Type type => PacketSerializer.GetEdgeQLType(type) ?? type.GetEdgeDBTypeName(),
                _ => obj.ToString()!
            };
        }

        public static string GenerateRandomVariableName()
        {
            return new string(Enumerable.Repeat(VARIABLE_CHARS, 12).Select(x => x[_rng.Next(x.Length)]).ToArray());
        }

        public static async ValueTask<IEnumerable<PropertyInfo>> GetPropertiesAsync<TType>(IEdgeDBQueryable edgedb, bool? exclusive = null, bool? @readonly = null, CancellationToken token = default)
        {
            var introspection = await SchemaIntrospector.GetOrCreateSchemaIntrospectionAsync(edgedb, token).ConfigureAwait(false);

            return GetProperties(introspection, typeof(TType), exclusive, @readonly);
        }

        public static IEnumerable<PropertyInfo> GetProperties(SchemaInfo schemaInfo, Type type, bool? exclusive = null, bool? @readonly = null)
        {
            if (!schemaInfo.TryGetObjectInfo(type, out var info))
                throw new NotSupportedException($"Cannot use {type.Name} as there is no schema information for it.");
            
            var props = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);
            return props.Where(x =>
            {
                var edgedbName = x.GetEdgeDBPropertyName();
                return info.Properties!.Any(x => x.Name == edgedbName &&
                    (!exclusive.HasValue || x.IsExclusive == exclusive.Value) &&
                    (!@readonly.HasValue || x.IsReadonly == @readonly.Value));
            });
        }

        public static Expression GenerateInsertShapeExpression(object? value, Type type)
        {
            var props = type.GetProperties()
                            .Where(x => 
                                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null && 
                                x.GetValue(value) != ReflectionUtils.GetDefault(x.PropertyType));

            return Expression.MemberInit(
                Expression.New(type),
                props.Select(x =>
                    Expression.Bind(x, Expression.MakeMemberAccess(Expression.Constant(value), x))
                    )
                );
        }

        public static async ValueTask<Expression<Func<TType, TType>>> GenerateUpdateFactoryAsync<TType>(IEdgeDBQueryable edgedb, TType inst, CancellationToken token = default)
        {
            var props = await GetPropertiesAsync<TType>(edgedb, @readonly: false, token: token).ConfigureAwait(false);

            props = props.Where(x => x.GetValue(inst) != ReflectionUtils.GetDefault(x.PropertyType));

            return Expression.Lambda<Func<TType, TType>>(
                Expression.MemberInit(
                    Expression.New(typeof(TType)), props.Select(x =>
                        Expression.Bind(x, Expression.MakeMemberAccess(Expression.Constant(inst), x)))
                    ),
                Expression.Parameter(typeof(TType), "x")
            );
        }

        public static async ValueTask<Expression<Func<TType, QueryContext, bool>>> GenerateUpdateFilterAsync<TType>(IEdgeDBQueryable edgedb, TType inst, CancellationToken token = default)
        {
            // try and get object id
            if (QueryObjectManager.TryGetObjectId(inst, out var id))
                return (_, ctx) => ctx.UnsafeLocal<Guid>("id") == id;

            // get exclusive properties.
            var exclusiveProperties = await GetPropertiesAsync<TType>(edgedb, exclusive: true, token: token).ConfigureAwait(false);

            var unsafeLocalMethod = typeof(QueryContext).GetMethod("UnsafeLocal")!;
            return Expression.Lambda<Func<TType, QueryContext, bool>>(
                exclusiveProperties.Select(x =>
                {

                    return Expression.Equal(
                        Expression.Call(
                            Expression.Parameter(typeof(QueryContext), "ctx"), 
                            unsafeLocalMethod, 
                            Expression.Constant(x.GetEdgeDBPropertyName())
                        ),
                        Expression.MakeMemberAccess(Expression.Parameter(typeof(TType), "x"), x)
                    );
                }).Aggregate((x, y) => Expression.And(x, y)),
                Expression.Parameter(typeof(QueryContext), "ctx"),
                Expression.Parameter(typeof(TType), "x")
            );
        }
    }
}
