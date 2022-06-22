using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class SelectNode : QueryNode<SelectContext>
    {
        public override bool IsRootNode => true;

        public override QueryExpressionType? ValidChildren
            => QueryExpressionType.Filter | QueryExpressionType.OrderBy | QueryExpressionType.Offset | QueryExpressionType.Limit;

        public SelectNode(QueryBuilder builder) : base(builder) { }

        protected virtual string GetShape()
        {
            var properties = Context.CurrentType.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            var propertyNames = properties.Select(x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? TypeBuilder.NamingStrategy.GetName(x));

            return $"{{ {string.Join(", ", propertyNames)} }}";
        }

        public override void Visit()
        {
            var shape = GetShape();
            Query.Append($"select {Context.SelectName ?? Context.CurrentType.GetEdgeDBTypeName()} {shape}");
        }

        public override void FinalizeQuery()
        {
            // check if theres a child select with the same type
            if(Builder.Nodes.Any(x => x is SelectNode select && select.Context.CurrentType == Context.CurrentType))
            {
                Query.Insert(7, "detached ");
            }
        }
    }
}
