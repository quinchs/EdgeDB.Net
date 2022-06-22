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
    public partial class QueryableCollection<TQueryResult>
    {
        private readonly IEdgeDBQueryable _edgedb;
        private readonly List<List<QueryNode>> _nodes;
        private List<QueryNode>? CurrentNodeGroup => _nodes.LastOrDefault();
        private QueryNode? CurrentRootNode => CurrentNodeGroup?.FirstOrDefault(x => x.IsRootNode);

        private static readonly Dictionary<Type, QueryExpressionType> _nodeTypes = new()
        {
            {typeof(SelectNode), QueryExpressionType.Select },
            {typeof(FilterNode), QueryExpressionType.Filter },
            {typeof(OrderByNode), QueryExpressionType.OrderBy },
            {typeof(InsertNode), QueryExpressionType.Insert },
            {typeof(WithNode), QueryExpressionType.With },
            {typeof(UnlessConflictOnNode), QueryExpressionType.UnlessConflictOn }
        };

        static QueryableCollection()
        {
            QueryObjectManager.Initialize();
        }

        internal QueryableCollection(IEdgeDBQueryable edgedb)
        {
            _edgedb = edgedb;
            _nodes = new();
        }

        private void AddNode<TNode>(QueryContext context, bool validate = true)
            where TNode : QueryNode
        {
            if(validate)
                ValidateNode<TNode>();

            // create the node and a builder
            var builder = new QueryBuilder(context, CurrentNodeGroup);
            var node = (QueryNode)Activator.CreateInstance(typeof(TNode), builder)!;

            // visit the node
            node.Visit();

            // create a new group if its a root node
            if (node.IsRootNode)
                _nodes.Add(new List<QueryNode>() { node });
            else if (CurrentNodeGroup != null)
                CurrentNodeGroup.Add(node);
            else
                throw new Exception("No node group found! this is a bug");
        }

        private void ValidateNode<TNode>()
            where TNode : QueryNode
        {
            if (!_nodeTypes.TryGetValue(typeof(TNode), out var type))
                throw new ArgumentException($"No matching node found for {typeof(TNode).Name}");

            if (CurrentRootNode == null)
                return;

            if ((CurrentRootNode.ValidChildren & type) == 0)
                throw new ArgumentException($"{type} cannot follow a {CurrentRootNode}");
        }

        public (string Query, IDictionary<string, object?> Parameters) Build()
        {
            List<string> query = new();
            List<IDictionary<string, object?>> parameters = new();
            Dictionary<string, object?> globals = new();

            var nodes = _nodes;

            // extract a with block or create one
            var withBlock = nodes.FirstOrDefault()?.FirstOrDefault();

            if(withBlock is not null and not WithNode)
            {
                var context = new WithContext(typeof(TQueryResult))
                {
                    Values = globals
                };
                nodes = nodes.Prepend(new List<QueryNode>() { new WithNode(new QueryBuilder(context)) }).ToList();
            }

            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var subNodes = nodes[i];
                for (int j = subNodes.Count - 1; j >= 0; j--)
                {
                    var node = subNodes[j];

                    if(node is WithNode withNode)
                    {
                        if (!withNode.HasVisited)
                            withNode.Visit();
                    }

                    node.FinalizeQuery();

                    var result = node.Build();

                    if(!string.IsNullOrEmpty(result.Query))
                        query.Add(result.Query);
                    
                    parameters.Add(result.Parameters);

                    foreach (var global in result.Globals)
                        globals[global.Key] = global.Value;
                }
            }

            query.Reverse();
            
            
            
            return (string.Join(' ', query), parameters.SelectMany(x => x).DistinctBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value));
        }
    }

    [Flags]
    public enum QueryExpressionType
    {
        Start = 0,
        Select = 1 << 0,
        Insert = 1 << 1,
        Update = 1 << 2,
        Delete = 1 << 3,
        With = 1 << 4,
        For = 1 << 5,
        Filter = 1 << 6,
        OrderBy = 1 << 7,
        Offset = 1 << 8,
        Limit = 1 << 9,
        Set = 1 << 10,
        Transaction = 1 << 11,
        Union = 1 << 12,
        UnlessConflictOn = 1 << 13,
        Rollback = 1 << 14,
        Commit = 1 << 15,
        Else = 1 << 16,
    }
}
