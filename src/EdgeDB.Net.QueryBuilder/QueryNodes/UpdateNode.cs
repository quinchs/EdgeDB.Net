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

        public override void Visit()
        {
            Query.Append($"update {Context.UpdateName ?? Context.CurrentType.GetEdgeDBTypeName()}");
        }

        public override void FinalizeQuery()
        {
            var exp = ExpressionTranslator.Translate(Context.UpdateExpression!, Builder.QueryVariables);
            Query.Append($" set {{ {exp} }}");

            if (Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"));
                Query.Clear();
            }
        }

        public void Filter(LambdaExpression filter)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter), "No expression was passed in for a filter node");

            var parsedExpression = ExpressionTranslator.Translate(filter, Builder.QueryVariables);
            Query.Append($" filter {parsedExpression}");
        }
    }
}
