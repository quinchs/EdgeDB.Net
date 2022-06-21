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
        public SelectNode(QueryBuilder builder) : base(builder) { }

        protected virtual string GetShape()
        {
            var properties = Context.CurrentType.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            var propertyNames = properties.Select(x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? TypeBuilder.NamingStrategy.GetName(x));

            return $"{{ {string.Join(", ", propertyNames)} }}";
        }

        protected override void Visit()
        {
            var shape = GetShape();
            Query.Append($"select {Context.CurrentType.GetEdgeDBTypeName()} {shape}");
        }

        protected override void FinalizeQuery()
        {
            // check if theres a child select with the same type
            if(Builder.Nodes.Any(x => x.Node is SelectNode select && select.Context.CurrentType == Context.CurrentType))
            {
                Query.Insert(7, "detached ");
            }
        }
    }
}
