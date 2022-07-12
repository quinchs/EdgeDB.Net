using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///     Represents a 'DELETE' node
    /// </summary>
    internal class DeleteNode : SelectNode
    {
        /// <inheritdoc/>
        public DeleteNode(NodeBuilder builder) : base(builder)
        {
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            Query.Append($"delete {Context.SelectName ?? Context.CurrentType.GetEdgeDBTypeName()}");
        }
    }
}
