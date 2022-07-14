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
    /// <summary>
    ///     A class containing useful utilities for building queries.
    /// </summary>
    internal static class QueryUtils
    {
        private const string VARIABLE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly Random _rng = new();

        /// <summary>
        ///     Checks whether or not a type is a valid link type.
        /// </summary>
        /// <param name="type">The type to check whether or not its a link.</param>
        /// <param name="isMultiLink">
        ///     The out parameter which is <see langword="true"/> 
        ///     if the type is a 'multi link'; otherwise a 'single link'.
        /// </param>
        /// <param name="innerLinkType">The inner type of the multi link if <paramref name="isMultiLink"/> is <see langword="true"/>; otherwise <see langword="null"/>.</param>
        /// <returns>
        ///     <see langword="true"/> if the given type is a link; otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsLink(Type type, out bool isMultiLink, [MaybeNullWhen(false)]out Type? innerLinkType)
        {
            innerLinkType = null;
            isMultiLink = false;

            Type? enumerableType = null;
            if (type != typeof(string) && (enumerableType = type.GetInterfaces().FirstOrDefault(x => ReflectionUtils.IsInstanceOfGenericType(typeof(IEnumerable<>), x))) != null)
            {
                innerLinkType = enumerableType.GenericTypeArguments[0];
                isMultiLink = true;
                var result = IsLink(innerLinkType, out _, out var linkType);
                innerLinkType ??= linkType;
                return result;
            }

            return TypeBuilder.IsValidObjectType(type);
        }

        /// <summary>
        ///     Parses a given object into its equivilant edgeql form.
        /// </summary>
        /// <param name="obj">The object to parse.</param>
        /// <returns>The string representation for the given object.</returns>
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

        /// <summary>
        ///     Generates a random valid variable name for use in queries.
        /// </summary>
        /// <returns>A 12 character long random string.</returns>
        public static string GenerateRandomVariableName()
            => new string(Enumerable.Repeat(VARIABLE_CHARS, 12).Select(x => x[_rng.Next(x.Length)]).ToArray());

        /// <summary>
        ///     Gets a collection of properties based on flags.
        /// </summary>
        /// <typeparam name="TType">The type to get the properties on.</typeparam>
        /// <param name="edgedb">A client to preform introspection with.</param>
        /// <param name="exclusive">
        ///     <see langword="true"/> to return only exclusive properties.
        ///     <see langword="false"/> to exclude exclusive properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="readonly">
        ///     <see langword="true"/> to return only readonly properties.
        ///     <see langword="false"/> to exclude readonly properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a collection of <see cref="PropertyInfo"/>.
        /// </returns>
        public static async ValueTask<IEnumerable<PropertyInfo>> GetPropertiesAsync<TType>(IEdgeDBQueryable edgedb, bool? exclusive = null, bool? @readonly = null, CancellationToken token = default)
        {
            var introspection = await SchemaIntrospector.GetOrCreateSchemaIntrospectionAsync(edgedb, token).ConfigureAwait(false);

            return GetProperties(introspection, typeof(TType), exclusive, @readonly);
        }

        /// <summary>
        ///     Gets a collection of properties based on flags.
        /// </summary>
        /// <param name="schemaInfo">
        ///     The introspection data on which to cross reference property data.
        /// </param>
        /// <param name="type">The type to get the properties on.</param>
        /// <param name="exclusive">
        ///     <see langword="true"/> to return only exclusive properties.
        ///     <see langword="false"/> to exclude exclusive properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <param name="readonly">
        ///     <see langword="true"/> to return only readonly properties.
        ///     <see langword="false"/> to exclude readonly properties.
        ///     <see langword="null"/> to include either or.
        /// </param>
        /// <returns>A collection of <see cref="PropertyInfo"/>.</returns>
        /// <exception cref="NotSupportedException">
        ///     The given type was not found within the introspection data.
        /// </exception>
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

        /// <summary>
        ///     Generates a default insert shape expression for the given type and value.
        /// </summary>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="type">The type to generate the shape for.</param>
        /// <returns>
        ///     An <see cref="Expression"/> that contains the insert shape for the given type.
        /// </returns>
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

        /// <summary>
        ///     Generates a default update factory expression for the given type and value.
        /// </summary>
        /// <typeparam name="TType">The type to generate the shape for.</typeparam>
        /// <param name="edgedb">A client used to preform introspection with.</param>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a generated update factory expression.
        /// </returns>
        public static async ValueTask<Expression<Func<TType, TType>>> GenerateUpdateFactoryAsync<TType>(IEdgeDBQueryable edgedb, TType value, CancellationToken token = default)
        {
            var props = await GetPropertiesAsync<TType>(edgedb, @readonly: false, token: token).ConfigureAwait(false);

            props = props.Where(x => x.GetValue(value) != ReflectionUtils.GetDefault(x.PropertyType));

            return Expression.Lambda<Func<TType, TType>>(
                Expression.MemberInit(
                    Expression.New(typeof(TType)), props.Select(x =>
                        Expression.Bind(x, Expression.MakeMemberAccess(Expression.Constant(value), x)))
                    ),
                Expression.Parameter(typeof(TType), "x")
            );
        }

        /// <summary>
        ///     Generates a default filter for the given type.
        /// </summary>
        /// <typeparam name="TType">The type to generate the filter for.</typeparam>
        /// <param name="edgedb">A client used to preform introspection with.</param>
        /// <param name="value">The value of which to do member lookups on.</param>
        /// <param name="token">A cancellation token used to cancel the introspection query.</param>
        /// <returns>
        ///     A ValueTask representing the (a)sync operation of preforming the introspection query.
        ///     The result of the task is a generated filter expression.
        /// </returns>
        public static async ValueTask<Expression<Func<TType, QueryContext, bool>>> GenerateUpdateFilterAsync<TType>(IEdgeDBQueryable edgedb, TType value, CancellationToken token = default)
        {
            // try and get object id
            if (QueryObjectManager.TryGetObjectId(value, out var id))
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
