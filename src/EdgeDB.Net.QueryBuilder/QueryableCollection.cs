using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class QueryableCollection<TQueryResult>
    {
        private readonly IEdgeDBQueryable _edgedb;

        internal QueryableCollection(IEdgeDBQueryable edgedb)
        {
            _edgedb = edgedb;
        }

        public QueryableCollection<TQueryResult> Where(Expression<Func<TQueryResult, bool>> condition)
        {
            return this;
        }


    }

    public enum QueryExpressionType
    {
        Start,
        Select,
        Insert,
        Update,
        Delete,
        With,
        For,
        Filter,
        OrderBy,
        Offset,
        Limit,
        Set,
        Transaction,
        Union,
        UnlessConflictOn,
        Rollback,
        Commit,
        Else,

        // internal
        Variable,
    }
}
