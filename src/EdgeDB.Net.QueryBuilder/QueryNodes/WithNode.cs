using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class WithNode : QueryNode<WithContext>
    {
        public override bool IsRootNode => true;
        public bool HasVisited { get; private set; }
     
        public override QueryExpressionType? ValidChildren
            => QueryExpressionType.Select | QueryExpressionType.Insert | QueryExpressionType.Update | QueryExpressionType.Delete;
        
        public WithNode(QueryBuilder builder) : base(builder) { }

        public override void Visit()
        {
            HasVisited = true;
            
            if (Context.Values is null || !Context.Values.Any())
                return;

            Query.Append($"with {string.Join(", ", Context.Values.Select(x => $"{x.Key} := {QueryUtils.ParseObject(x.Value)}"))}");
        }
    }
}
