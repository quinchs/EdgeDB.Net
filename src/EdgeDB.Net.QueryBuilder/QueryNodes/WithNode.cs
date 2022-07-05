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

            foreach(var global in Context.Values)
            {
                var value = global.Value;

                if (value is IQueryBuilder queryBuilder)
                {
                    var query = queryBuilder.Build();
                    value = new SubQuery($"({query.Query})");

                    if(query.Parameters is not null)
                        foreach (var variable in query.Parameters)
                            SetVariable(variable.Key, variable.Value);

                    if (query.Globals is not null)
                        foreach (var queryGlobal in query.Globals)
                            SetGlobal(queryGlobal.Name, queryGlobal.Value, null);
                }

                if(value is SubQuery subQuery && subQuery.RequiresIntrospection)
                {
                    if (subQuery.RequiresIntrospection && SchemaInfo is null)
                        throw new InvalidOperationException("Cannot build without introspection! A node requires query introspection.");
                    value = subQuery.Build(SchemaInfo!);
                }

                values.Add($"{global.Name} := {QueryUtils.ParseObject(value)}");
            }

            Query.Append($"with {string.Join(", ", values)}");
        }
    }
}
