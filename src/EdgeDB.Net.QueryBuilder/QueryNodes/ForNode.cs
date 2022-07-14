using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///    Represents a 'FOR' node.
    /// </summary>
    internal class ForNode : QueryNode<ForContext>
    {
        /// <inheritdoc/>
        public ForNode(NodeBuilder builder) : base(builder)
        {
        }

        /// <summary>
        ///     Parsed the given contextual expression into an iterator.
        /// </summary>
        /// <param name="name">The name of the root iterator.</param>
        /// <param name="json">The json used for iteration.</param>
        /// <returns>
        ///     A edgeql iterator for a 'FOR' statement; or <see langword="null"/> 
        ///     if the iterator requires introspection to build.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     A type cannot be used as a parameter to a 'FOR' expression
        /// </exception>
        private string? ParseExpression(string name, string json)
        {
            // check if we're returning a query builder
            if (Context.Expression!.ReturnType == typeof(IQueryBuilder))
            {
                // parse our json value for processing by sub nodes.
                var jArray = JArray.Parse(json);

                // construct the parameters for the lambda
                var parameters = Context.Expression.Parameters.Select(x =>
                {
                    return x switch
                    {
                        _ when x.Type == typeof(QueryContext) => new QueryContext(),
                        _ when ReflectionUtils.IsInstanceOfGenericType(typeof(JsonVariable<>), x.Type)
                            => Activator.CreateInstance(typeof(JsonVariable<>).MakeGenericType(Context.CurrentType), name, jArray),
                        _ => throw new ArgumentException($"Cannot use {x.Type} as a parameter to a 'FOR' expression")
                    };
                }).ToArray();

                // build and compile our lambda to get the query builder instance
                var builder = (IQueryBuilder)Context.Expression!.Compile().DynamicInvoke(parameters)!;

                // copy the globals & variables to the current builder
                foreach (var global in builder.Globals)
                    SetGlobal(global.Name, global.Value, global.Reference);

                foreach (var variable in builder.Variables)
                    SetVariable(variable.Key, variable.Value);

                // add all nodes as sub nodes to this node
                SubNodes.AddRange(builder.Nodes);

                // return nothing indicating we need to do introspection
                return null;
            }
            else
                return ExpressionTranslator.Translate(Context.Expression!, Builder.QueryVariables, Context, Builder.QueryGlobals);
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            // pull the name of the value that the user has specified
            var name = Context.Expression!.Parameters.First(x => x.Type != typeof(QueryContext)).Name!;

            // serialize the collection & generate a name for the json variable
            var setJson = JsonConvert.SerializeObject(Context.Set);
            var jsonName = QueryUtils.GenerateRandomVariableName();

            // set the json variable
            SetVariable(jsonName, setJson);

            // append the 'FOR' statement
            Query.Append($"for {name} in <json>${jsonName} union ");

            // parse the iterator expression
            var parsed = ParseExpression(name, setJson);

            // if it's not null or empty, append the union statement's content
            if (!string.IsNullOrEmpty(parsed))
                Query.Append($"({parsed})");
            else
                RequiresIntrospection = true; // else tell the query builder that this node needs introspection
        }

        /// <inheritdoc/>
        public override void FinalizeQuery()
        {
            // finalize and build our sub nodes
            var iterator = SubNodes.Select(x =>
            {
                x.SchemaInfo = SchemaInfo;
                x.FinalizeQuery();

                // we don't need to copy variables or nodes here since we did that in the parse step
                return x.Build().Query;
            }).Aggregate((x, y) => $"{x} {y}");

            // append union statement's content
            Query.Append($"({iterator})");
        }
    }
}
