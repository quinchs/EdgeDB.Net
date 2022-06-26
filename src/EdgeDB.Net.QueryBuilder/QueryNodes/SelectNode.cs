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
        public const int MAX_DEPTH = 1;
        public SelectNode(NodeBuilder builder) : base(builder) { }

        private string GetShape(Type type, int currentDepth = 0)
        {
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            var propertyNames = properties.Select(x =>
            {
                var name = x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? TypeBuilder.NamingStrategy.GetName(x);
                if (TypeBuilder.IsValidObjectType(x.PropertyType))
                {
                    if(currentDepth < MAX_DEPTH)
                        return $"{name}: {GetShape(x.PropertyType, currentDepth + 1)}";
                    return null;
                }
                else
                {
                    return name;
                }
            }).Where(x => x != null);

            return $"{{ {string.Join(", ", propertyNames)} }}";
        }

        private string GetDefaultShape()
            => GetShape(Context.CurrentType);

        private string GetShape()
        {
            if(Context.Shape == null)
            {
                return GetDefaultShape();
            }

            // if its a call to a global
            if(Context.Shape.Body is MethodCallExpression)
            {
                var exp = ExpressionTranslator.Translate(Context.Shape, Builder.QueryVariables, Context);
                Context.SelectName = exp;
                return GetDefaultShape();
            }
            else if (Context.Shape.Body is NewExpression)
            {
                return ExpressionTranslator.Translate(Context.Shape, Builder.QueryVariables, Context);
            }

            return "";
            
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

            var parsedExpression = ExpressionTranslator.Translate(expression, Builder.QueryVariables, Context);
            Query.Append($" filter {parsedExpression}");
        }

        public void OrderBy(bool asc, LambdaExpression selector, OrderByNullPlacement? nullPlacement)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector), "No expression was passed in for an order by node");

            var parsedExpression = ExpressionTranslator.Translate(selector, Builder.QueryVariables, Context);
            var direction = asc ? "asc" : "desc";
            Query.Append($" order by {parsedExpression} {direction}{(nullPlacement.HasValue ? $" {nullPlacement.Value.ToString().ToLowerInvariant()}" : "")}");
        }

        internal void Offset(long offset)
        {
            Query.Append($" offset {offset}");
        }

        internal void OffsetExpression(LambdaExpression exp)
        {
            Query.Append($" offset {exp}");
        }

        internal void Limit(long limit)
        {
            Query.Append($" limit {limit}");
        }

        internal void LimitExpression(LambdaExpression exp)
        {
            Query.Append($" limit {exp}");
        }
    }
}
