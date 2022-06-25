using EdgeDB.Interfaces.Queries;
using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class SelectNode : QueryNode<SelectContext>
    {
        public SelectNode(NodeBuilder builder) : base(builder) { }

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

        public void Filter(LambdaExpression expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression), "No expression was passed in for a filter node");

            var parsedExpression = ExpressionTranslator.Translate(expression, Builder.QueryVariables);
            Query.Append($" filter {parsedExpression}");
        }

        public void OrderBy(bool asc, LambdaExpression selector, OrderByNullPlacement? nullPlacement)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector), "No expression was passed in for an order by node");

            var parsedExpression = ExpressionTranslator.Translate(selector, Builder.QueryVariables);
            var direction = asc ? "asc" : "desc";
            Query.Append($" order by {parsedExpression} {direction}{(nullPlacement.HasValue ? $" {nullPlacement.Value.ToString().ToLowerInvariant()}" : "")}");
        }

        internal void Offset(long offset)
        {
            Query.Append($" offset {offset}");
        }

        internal void Limit(long limit)
        {
            Query.Append($" limit {limit}");
        }
    }
}
