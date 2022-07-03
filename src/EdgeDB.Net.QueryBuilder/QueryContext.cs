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

        [EquivalentOperator(typeof(LocalReference))]
        public TType UnsafeLocal<TType>(string name)
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
    }
}
