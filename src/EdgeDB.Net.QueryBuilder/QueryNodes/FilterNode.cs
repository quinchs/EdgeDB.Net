using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class FilterNode : QueryNode<FilterContext>
    {
        public FilterNode(QueryBuilder builder) : base(builder) { }

        protected override void Visit()
        {
            if (Context.Expression is null)
                throw new ArgumentNullException("No expression was passed in for a filter node");

            var parsedExpression = ExpressionTranslator.Translate(Context.Expression);
            Query.Append($"filter {parsedExpression}");
        }
    }
}
