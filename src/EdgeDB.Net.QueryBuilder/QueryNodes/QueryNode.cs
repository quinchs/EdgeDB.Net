using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal abstract class QueryNode
    {
        protected readonly QueryBuilder Builder;

        protected StringBuilder Query
            => Builder.Query;

        protected QueryContext Context
            => Builder.Context;

        public QueryNode(QueryBuilder builder)
        {
            Builder = builder;
        }

        protected abstract void Visit();
        protected abstract void FinalizeQuery();

        protected void SetVariable(string name, object? value)
            => Builder.QueryVariables[name] = value;
    }
}
