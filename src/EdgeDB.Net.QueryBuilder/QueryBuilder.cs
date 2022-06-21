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
        public IReadOnlyCollection<CompiledQueryNode> Nodes { get; }
        public QueryContext Context { get; }
        public Dictionary<string, object?> QueryVariables { get; } = new();

        public QueryBuilder(QueryContext context, IReadOnlyCollection<CompiledQueryNode>? nodes = null)
        {
            Query = new();
            Nodes = nodes ?? Array.Empty<CompiledQueryNode>();
            Context = context;
        }
    }

    internal class CompiledQueryNode
    {
        public QueryNode? Node { get; init; }
        public StringBuilder? Query { get; init; }
    }
}
