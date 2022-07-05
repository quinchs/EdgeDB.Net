﻿using EdgeDB.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal abstract class QueryNode<TContext> : QueryNode
        where TContext : NodeContext
    {
        protected QueryNode(NodeBuilder builder) : base(builder) { }

        internal new TContext Context
            => (TContext)Builder.Context;
    }

    internal abstract class QueryNode
    {
        public bool IsAutoGenerated
            => Builder.IsAutoGenerated;

        public bool RequiresIntrospection { get; protected set; }
        public SchemaInfo? SchemaInfo { get; set; }
        internal List<int> ReferencedGlobals { get; } = new();
        internal List<string> ReferencedVariables { get; } = new();
        internal List<QueryNode> SubNodes { get; } = new();

        internal readonly NodeBuilder Builder;
        internal StringBuilder Query
            => Builder.Query;

        internal NodeContext Context
            => Builder.Context;

        public QueryNode(NodeBuilder builder)
        {
            Builder = builder;
        }

        public abstract void Visit();
        public virtual void FinalizeQuery() { }

        protected void SetVariable(string name, object? value)
        {
            ReferencedVariables.Add(name);
            Builder.QueryVariables[name] = value;
        }

        protected void SetGlobal(string name, object? value, object? reference)
        {
            var global = new QueryGlobal(name, value, reference);
            Builder.QueryGlobals.Add(global);
            ReferencedGlobals.Add(Builder.QueryGlobals.IndexOf(global));
        }

        protected string GetOrAddGlobal(object? reference, object? value)
        {
            var global = Builder.QueryGlobals.FirstOrDefault(x => x.Value == value);
            if (global != null)
                return global.Name;
            var name = QueryUtils.GenerateRandomVariableName();
            SetGlobal(name, value, reference);
            return name;
        }

        internal BuiltQueryNode Build()
            => new(Query.ToString(), Builder.QueryVariables);
    }

    internal class BuiltQueryNode
    {
        public string Query { get; init; }
        public IDictionary<string, object?> Parameters { get; init; }

        public BuiltQueryNode(string query, IDictionary<string, object?> parameters)
        {
            Query = query;
            Parameters = parameters;
        }
    }
}
