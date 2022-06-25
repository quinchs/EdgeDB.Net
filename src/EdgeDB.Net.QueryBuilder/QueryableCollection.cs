using EdgeDB.Interfaces.Queries;
using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class QueryableCollection<TType> : QueryBuilder<TType>
    {
        private readonly IEdgeDBQueryable _edgedb;

        internal QueryableCollection(IEdgeDBQueryable edgedb)
        {
            _edgedb = edgedb;
        }
    }
}
