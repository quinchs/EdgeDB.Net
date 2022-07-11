using EdgeDB.Interfaces;
using EdgeDB.Interfaces.Queries;
using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents context used within query functions.
    /// </summary>
    public sealed class QueryContext
    {
        /// <summary>
        ///     References a defined query global given a name.
        /// </summary>
        /// <typeparam name="TType">The type of the global.</typeparam>
        /// <param name="name">The name of the global.</param>
        /// <returns>
        ///     A mock reference to a global with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(VariablesReference))]
        public TType Global<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public TType Local<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public object? Local(string name)
            => default!;

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <typeparam name="TType">The type of the local.</typeparam>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public TType UnsafeLocal<TType>(string name)
            => default!;

        /// <summary>
        ///     References a contextual local without checking the local context.
        /// </summary>
        /// <param name="name">The name of the local.</param>
        /// <returns>
        ///     A mock reference to a local with the given <paramref name="name"/>.
        /// </returns>
        [EquivalentOperator(typeof(LocalReference))]
        public object? UnsafeLocal(string name)
            => default!;

        public TType Raw<TType>(string query)
            => default!;

        /// <summary>
        ///     Includes a property within a shape.
        /// </summary>
        /// <typeparam name="TType">The type of the property.</typeparam>
        /// <returns>
        ///     A mock reference to the property that this include statement is being assigned to.
        /// </returns>
        public TType Include<TType>()
            => default!;

        public TType IncludeLink<TType>(Expression<Func<TType>> shape)
            => default!;

        public TType[] IncludeMultiLink<TType>(Expression<Func<TType>> shape)
            => default!;

        public TCollection IncludeMultiLink<TType, TCollection>(Expression<Func<TType>> shape)
            where TCollection : IEnumerable<TType>
            => default!;

        public EdgeDBObject[] BackLink(string property)
            => default!;

        public TCollection BackLink<TCollection>(string property)
            where TCollection : IEnumerable<EdgeDBObject>
            => default!;

        public TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector)
            => default!;

        public TType[] BackLink<TType>(Expression<Func<TType, object?>> propertySelector, Expression<Func<TType>> shape)
            => default!;

        public TCollection BackLink<TType, TCollection>(Expression<Func<TType, object?>> propertySelector, Expression<Func<TType>> shape)
            where TCollection : IEnumerable<TType>
            => default!;

        public TType SubQuery<TType>(ISingleCardinalityQuery<TType> query)
            => default!;

        public TType[] SubQuery<TType>(IMultiCardinalityQuery<TType> query)
            => default!;

        public TCollection SubQuery<TType, TCollection>(IMultiCardinalityQuery<TType> query)
            where TCollection : IEnumerable<TType>
            => default!;
    }
}
