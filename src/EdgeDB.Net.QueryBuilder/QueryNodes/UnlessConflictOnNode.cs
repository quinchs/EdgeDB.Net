using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class UnlessConflictOnNode : QueryNode<UnlessConflictOnContext>
    {
        public override bool IsRootNode => false;

        public UnlessConflictOnNode(QueryBuilder builder) : base(builder) { }

        public override void Visit()
        {
            Query.Append($"unless conflict on {ExpressionTranslator.Translate(Context.Selector!)}");
        }
    }
}
