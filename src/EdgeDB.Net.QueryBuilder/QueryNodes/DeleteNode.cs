using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class DeleteNode : SelectNode
    {
        public DeleteNode(NodeBuilder builder) : base(builder)
        {
        }

        public override void Visit()
        {
            Query.Append($"delete {Context.SelectName ?? Context.CurrentType.GetEdgeDBTypeName()}");
        }
    }
}
