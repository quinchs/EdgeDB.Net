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

        public virtual QueryExpressionType? ValidChildren { get; }
        public abstract bool IsRootNode { get; }

        protected StringBuilder Query
            => Builder.Query;

        public QueryNode(QueryBuilder builder)
        {
            Builder = builder;
        }

        public abstract void Visit();
        public virtual void FinalizeQuery() { }

        protected void SetVariable(string name, object? value)
            => Builder.QueryVariables[name] = value;

        protected void SetGlobal(string name, object? value)
            => Builder.QueryGlobals[name] = value;

        internal BuiltQueryNode Build()
            => new BuiltQueryNode(Query.ToString(), Builder.QueryVariables, Builder.QueryGlobals);
    }

    internal class BuiltQueryNode
    {
        public string Query { get; init; }
        public IDictionary<string, object?> Parameters { get; init; }
        public IDictionary<string, object?> Globals { get; init; }

        public BuiltQueryNode(string query, IDictionary<string, object?> parameters, IDictionary<string, object?> globals)
        {
            Query = query;
            Parameters = parameters;
            Globals = globals;
        }
    }
}
