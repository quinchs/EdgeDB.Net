using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal abstract class QueryNode<TContext> : QueryNode
        where TContext : QueryContext
    {
        protected QueryNode(QueryBuilder builder) : base(builder) { }

        protected TContext Context
            => (TContext)Builder.Context;
    }

    internal abstract class QueryNode
    {
        protected readonly QueryBuilder Builder;

        protected StringBuilder Query
            => Builder.Query;

        public QueryNode(QueryBuilder builder)
        {
            Builder = builder;
        }

        protected abstract void Visit();
        protected virtual void FinalizeQuery() { }

        protected void SetVariable(string name, object? value)
            => Builder.QueryVariables[name] = value;
    }
}
