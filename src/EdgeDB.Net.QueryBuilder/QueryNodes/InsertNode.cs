using EdgeDB.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    /// <summary>
    ///    Represents a 'INSERT' node.
    /// </summary>
    internal class InsertNode : QueryNode<InsertContext>
    {
        /// <summary>
        ///     Whether or not to autogenerate the unless conflict clause.
        /// </summary>
        private bool _autogenerateUnlessConflict;

        /// <summary>
        ///     The else clause if any.
        /// </summary>
        private readonly StringBuilder _elseStatement;

        /// <summary>
        ///     The list of currently inserted types used to determine if 
        ///     a nested query can be preformed.
        /// </summary>
        private readonly List<Type> _subQueryMap = new();
        
        /// <inheritdoc/>
        public InsertNode(NodeBuilder builder) : base(builder) 
        {
            _elseStatement = new();
        }

        /// <summary>
        ///     Builds an insert shape based on the given type and value.
        /// </summary>
        /// <param name="shapeType">The type to build the shape for.</param>
        /// <param name="shapeValue">The value to build the shape with.</param>
        /// <returns>The built insert shape.</returns>
        /// <exception cref="InvalidOperationException">
        ///     No serialization method could be found for a property.
        /// </exception>
        private string BuildInsertShape(Type? shapeType = null, object? shapeValue = null)
        {
            List<string> shape = new();
            
            // use the provide shape and value if they're not null, otherwise
            // use the ones defined in context
            var type = shapeType ?? Context.CurrentType;
            var value = shapeValue ?? Context.Value;

            // if the value is an expression we can just directly translate it
            if (value is LambdaExpression expression)
                return $"{{ {ExpressionTranslator.Translate(expression, Builder.QueryVariables, Context, Builder.QueryGlobals)} }}";

            // get all properties that aren't marked with the EdgeDBIgnore attribute
            var properties = type.GetProperties().Where(x => x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null);

            foreach(var property in properties)
            {
                // define the type and whether or not it's a link
                var propType = property.PropertyType;
                var isLink = QueryUtils.IsLink(property.PropertyType, out var isArray, out var innerType);
                
                // get the equivalent edgedb property name
                var propertyName = property.GetEdgeDBPropertyName();
                
                // get the scalar type
                var edgeqlType = PacketSerializer.GetEdgeQLType(propType);

                // if a scalar type is found for the property type
                if(edgeqlType != null)
                {
                    // set it as a variable and continue the iteration
                    var varName = QueryUtils.GenerateRandomVariableName();
                    SetVariable(varName, property.GetValue(value));
                    shape.Add($"{propertyName} := <{edgeqlType}>${varName}");
                    continue;
                }

                // if the property is a link
                if (isLink)
                {
                    // get the value
                    var subValue = property.GetValue(value);

                    // if its null we can append an empty set
                    if (subValue is null)
                        shape.Add($"{propertyName} := {{}}");
                    else if (isArray) // if its a multi link
                    {
                        List<string> subShape = new();

                        // iterate over all values and generate their resolver
                        foreach (var item in (IEnumerable)subValue!)
                        {
                            subShape.Add(BuildLinkResolver(innerType!, item));
                        }

                        // append the sub-shape
                        shape.Add($"{propertyName} := {{ {string.Join(", ", subShape)} }}");
                    }
                    else // generate the link resolver and append it
                        shape.Add($"{propertyName} := {BuildLinkResolver(propType, subValue)}");

                    continue;
                }

                throw new InvalidOperationException($"Failed to find method to serialize the property \"{property.PropertyType.Name}\" on type {type.Name}");
            }

            return $"{{ {string.Join(", ", shape)} }}";
        }

        /// <summary>
        ///     Resolves a sub query for a link.
        /// </summary>
        /// <param name="type">The type of the link</param>
        /// <param name="value">The value of the link.</param>
        /// <returns>
        ///     A sub query or global name to reference the links value within the query.
        /// </returns>
        private string BuildLinkResolver(Type type, object? value)
        {
            // if the value is null we can just return an empty set
            if (value is null)
                return "{}";

            // is it a value thats been returned from a previous query?
            if (QueryObjectManager.TryGetObjectId(value, out var id))
            {
                // add a sub select statement
                return InlineOrGlobal(
                    type,
                    new SubQuery($"(select {type.GetEdgeDBTypeName()} filter .id = <uuid>\"{id}\")"),
                    value);
            }
            else
            {
                RequiresIntrospection = true;
             
                // add a insert select statement
                return InlineOrGlobal(type, new SubQuery((info) =>
                {
                    var name = type.GetEdgeDBTypeName();
                    var exclusiveProps = QueryUtils.GetProperties(info, type, true);
                    var exclusiveCondition = exclusiveProps.Any() ?
                        $" unless conflict on {(exclusiveProps.Count() > 1 ? $"({string.Join(", ", exclusiveProps.Select(x => $".{x.GetEdgeDBPropertyName()}"))})" : $".{exclusiveProps.First().GetEdgeDBPropertyName()}")} else (select {name})"
                        : string.Empty;
                    return $"(insert {name} {BuildInsertShape(type, value)}{exclusiveCondition})";
                }), value);
            }
        }

        /// <summary>
        ///     Adds a sub query as an inline query or as a global depending on if the current 
        ///     query contains any statements for the provided type.
        /// </summary>
        /// <param name="type">The returning type of the sub query.</param>
        /// <param name="value">The query itself.</param>
        /// <param name="reference">The optional reference object.</param>
        /// <returns>
        ///     A sub query or global name to reference the sub query.
        /// </returns>
        private string InlineOrGlobal(Type type, SubQuery value, object? reference)
        {
            // if were in a query with the type or the query requires introspection add it as a global
            if (_subQueryMap.Contains(type) || (value is SubQuery sq && sq.RequiresIntrospection))
                return GetOrAddGlobal(reference, value);

            // add it to our sub query map and return the inlined version
            _subQueryMap.Add(type);
            return value is SubQuery subQuery && subQuery.Query != null
                ? subQuery.Query
                : value.ToString()!;
        }

        /// <inheritdoc/>
        public override void Visit()
        {
            // add the current type to the sub query map
            _subQueryMap.Add(Context.CurrentType);

            // build the insert shape
            var shape = BuildInsertShape();

            // append it to our query
            Query.Append($"insert {Context.CurrentType.GetEdgeDBTypeName()} {shape}");
        }

        /// <inheritdoc/>
        public override void FinalizeQuery()
        {
            // if we require autogeneration of the unless conflict statement
            if(_autogenerateUnlessConflict)
            {
                if (SchemaInfo is null)
                    throw new NotSupportedException("Cannot use autogenerated unless conflict on without schema interpolation");

                if (!SchemaInfo.TryGetObjectInfo(Context.CurrentType, out var typeInfo))
                    throw new NotSupportedException($"Could not find type info for {Context.CurrentType}");

                // get all exclusive properties that aren't the id property
                var exclusiveProperties = typeInfo.Properties?.Where(x => x.IsExclusive && x.Name != "id");

                if (exclusiveProperties == null || !exclusiveProperties.Any())
                    throw new NotSupportedException($"The type {typeInfo.Name} does not have any user defined exclusive properties");

                // build the constraints
                // TODO: (.prop1, .prop2) isn't valid for multiple contraints.
                var constraint = exclusiveProperties.Count() > 1 ?
                    $"({string.Join(", ", exclusiveProperties.Select(x => $".{x.Name}"))})" :
                    $".{exclusiveProperties.First().Name}";

                Query.Append($" unless conflict on {constraint}");
            }
            else
                Query.Append(_elseStatement);

            // if the query builder wants this node as a global
            if (Context.SetAsGlobal && Context.GlobalName != null)
            {
                SetGlobal(Context.GlobalName, new SubQuery($"({Query})"), null);
                Query.Clear();
            }
        }

        /// <summary>
        ///     Adds a unless conflict on (...) statement to the insert node
        /// </summary>
        /// <remarks>
        ///     This method requires introspection on the <see cref="FinalizeQuery"/> step.
        /// </remarks>
        public void UnlessConflict()
        {
            _autogenerateUnlessConflict = true;
            RequiresIntrospection = true;
        }

        /// <summary>
        ///     Adds a unless conflict on statement to the insert node
        /// </summary>
        /// <param name="selector">The property selector for the conflict clause.</param>
        public void UnlessConflictOn(LambdaExpression selector)
        {
            Query.Append($" unless conflict on {ExpressionTranslator.Translate(selector, Builder.QueryVariables, Context, Builder.QueryGlobals)}");
        }

        /// <summary>
        ///     Adds the default else clause to the insert node that returns the conflicting object.
        /// </summary>
        public void ElseDefault()
        {
            _elseStatement.Append($" else (select {Context.CurrentType.GetEdgeDBTypeName()})");
        }

        /// <summary>
        ///     Adds a else statement to the insert node.
        /// </summary>
        /// <param name="builder">The builder that contains the else statement.</param>
        public void Else(IQueryBuilder builder)
        {
            // remove addon & autogen nodes.
            var userNodes = builder.Nodes.Where(x => !builder.Nodes.Any(y => y.SubNodes.Contains(x)) || !x.IsAutoGenerated);

            // TODO: better checks for this, future should add a callback to add the
            // node with its context so any parent builder can change contexts for nodes
            foreach (var node in userNodes)
                node.Context.SetAsGlobal = false;

            foreach(var variable in builder.Variables)
            {
                Builder.QueryVariables[variable.Key] = variable.Value;
            }

            var newBuilder = new QueryBuilder<object?>(userNodes.ToList(), builder.Globals.ToList(), builder.Variables.ToDictionary(x => x.Key, x=> x.Value));

            var result = newBuilder.BuildWithGlobals();
            _elseStatement.Append($" else ({result.Query})");
        }
    }
}
