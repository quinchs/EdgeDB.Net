﻿using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents a builder used by <see cref="QueryNode"/>s to build a section of a query.
    /// </summary>
    internal class NodeBuilder
    {
        /// <summary>
        ///     Gets the string builder for the current nodes query.
        /// </summary>
        public StringBuilder Query { get; }

        /// <summary>
        ///     Gets a collection of nodes currently within the builder.
        /// </summary>
        public List<QueryNode> Nodes { get; }

        /// <summary>
        ///     Gets the node context for the current builder.
        /// </summary>
        public NodeContext Context { get; }

        /// <summary>
        ///     Gets the query variable collection used to add new variables.
        /// </summary>
        public Dictionary<string, object?> QueryVariables { get; }

        /// <summary>
        ///     Gets the query global collection used to add new globals.
        /// </summary>
        public List<QueryGlobal> QueryGlobals { get; }

        /// <summary>
        ///     Gets whether or not the current node is auto generated.
        /// </summary>
        public bool IsAutoGenerated { get; init; }

        /// <summary>
        ///     Constructs a new <see cref="NodeBuilder"/>.
        /// </summary>
        /// <param name="context">The context for the node this builder is being supplied to.</param>
        /// <param name="globals">The global collection.</param>
        /// <param name="nodes">The collection of defined nodes.</param>
        /// <param name="variables">The variable collection.</param>
        public NodeBuilder(NodeContext context, List<QueryGlobal> globals, List<QueryNode>? nodes = null, Dictionary<string, object?>? variables = null)
        {
            Query = new();
            Nodes = nodes ?? new();
            Context = context;
            QueryGlobals = globals;
            QueryVariables = variables ?? new();
        }
    }
}