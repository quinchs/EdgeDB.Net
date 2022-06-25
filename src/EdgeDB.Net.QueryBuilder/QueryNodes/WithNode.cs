using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class WithNode : QueryNode<WithContext>
    {
        public bool HasVisited { get; private set; }

        public WithNode(NodeBuilder builder) : base(builder) { }

        public override void Visit()
        {
            HasVisited = true;
            
            if (Context.Values is null || !Context.Values.Any())
                return;

            List<string> values = new();

            foreach(var kvp in Context.Values)
            {
                var value = kvp.Value;

                if (value is IQueryBuilder queryBuilder)
                {
                    var subQuery = queryBuilder.Build();
                    value = new SubQuery($"({subQuery.Query})");
                    foreach (var variable in subQuery.Parameters)
                        SetVariable(variable.Key, variable.Value);
                }

                values.Add($"{kvp.Key} := {QueryUtils.ParseObject(value)}");
            }

            Query.Append($"with {string.Join(", ", values)}");
        }
    }
}
