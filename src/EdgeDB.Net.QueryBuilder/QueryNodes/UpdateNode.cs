using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class UpdateNode : QueryNode<UpdateContext>
    {
        public UpdateNode(NodeBuilder builder) : base(builder) { }

        private string? _translatedExpression;

        public override void Visit()
        {
            Query.Append($"update {Context.UpdateName ?? Context.CurrentType.GetEdgeDBTypeName()}");
            _translatedExpression = ExpressionTranslator.Translate(Context.UpdateExpression!, Builder.QueryVariables, Context, Builder.QueryGlobals);

            RequiresIntrospection = Context.ChildQueries.Any(x => x.Value.RequiresIntrospection);
        }

        public override void FinalizeQuery()
        {
            Query.Append($" set {{ {_translatedExpression} }}");

            if (RequiresIntrospection && SchemaInfo is null)
                throw new InvalidOperationException("This node requires schema introspection but none was provided");

            foreach (var child in Context.ChildQueries)
                SetGlobal(child.Key, child.Value, null); // sub query will be built with introspection by the 'With' node.


            if (Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"), null);
                Query.Clear();
            }
        }

        public void Filter(LambdaExpression filter)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter), "No expression was passed in for a filter node");

            var parsedExpression = ExpressionTranslator.Translate(filter, Builder.QueryVariables, Context, Builder.QueryGlobals);
            Query.Append($" filter {parsedExpression}");
        }
    }
}
