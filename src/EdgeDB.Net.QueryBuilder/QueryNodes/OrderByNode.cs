using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class OrderByNode : QueryNode<OrderByContext>
    {
        public override bool IsRootNode => false;
        public OrderByNode(QueryBuilder builder) : base(builder) { }

        public override void FinalizeQuery()
        {
            // check if previous node was a order by, if so change to 'then'
            if (Builder.Nodes.FirstOrDefault() is OrderByNode)
            {
                Query.Remove(0, 8);
                Query.Insert(0, "then");
            }
        }

        public override void Visit()
        {
            var expression = ExpressionTranslator.Translate(Context.Expression!);
            var direction = Context.Direction == OrderByDirection.Ascending ? "asc" : "desc";
            Query.Append($"order by {expression} {direction}");

            if (Context.EmptyPlacement.HasValue)
            {
                Query.Append($" empty {Context.EmptyPlacement.Value.ToString().ToLowerInvariant()}");
            }
        }
    }
}
