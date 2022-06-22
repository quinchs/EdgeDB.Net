using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryBuilder
    {
        public StringBuilder Query { get; }
        public List<QueryNode> Nodes { get; }
        public QueryContext Context { get; }
        public Dictionary<string, object?> QueryVariables { get; } = new();
        public Dictionary<string, object?> QueryGlobals { get; } = new();

        public QueryBuilder(QueryContext context, List<QueryNode>? nodes = null)
        {
            Query = new();
            Nodes = nodes ?? new();
            Context = context;
        }
    }
}
