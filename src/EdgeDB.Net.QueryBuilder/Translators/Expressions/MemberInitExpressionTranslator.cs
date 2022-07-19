using EdgeDB.QueryNodes;
using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression calling a 
    ///     constructor and initializing one or more members of the new object.
    /// </summary>
    internal class MemberInitExpressionTranslator : ExpressionTranslator<MemberInitExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(MemberInitExpression expression, ExpressionContext context)
        {
            List<string> initializations = new();

            foreach(var binding in expression.Bindings)
            {
                switch (binding)
                {
                    // member assignment ex: 'Property = value'
                    case MemberAssignment assignment:
                        {
                            // get the members type and edgedb equivalent name
                            var memberType = assignment.Member.GetMemberType();
                            var memberName = assignment.Member.GetEdgeDBPropertyName();

                            // if its a link property
                            if (QueryUtils.IsLink(memberType, out var isArray, out var innerType))
                            {
                                if (isArray)
                                    memberType = innerType!;

                                // Check if we're setting a link property here, if we're in a select node and its a shape def
                                // we should let it parse. If not what should happen is globals should be 
                                // defined for this object and the members value with an update for the current contextual object.
                                // to do this we need to add some callback for the context to tell the query builder our intent.
                                if (context.NodeContext is SelectContext &&
                                    assignment.Expression is MethodCallExpression mcx &&
                                    mcx.Method.DeclaringType == typeof(QueryContext))
                                {
                                    initializations.Add($"{memberName}: {{ {TranslateExpression(assignment.Expression, context)} }}");
                                    break;
                                }

                                // get the name of the type.
                                var typeName = memberType.GetEdgeDBTypeName();

                                if (context.NodeContext is not UpdateContext updateContext)
                                    throw new NotSupportedException("Cannot set links with the current node");

                                SubQuery? subQuery = null;
                                bool isSubQuery = true;

                                // figure out what type of sub query we're going to make for this link property
                                switch (assignment.Expression)
                                {
                                    // when its a direct value reference
                                    case MemberExpression memberAccess when (memberAccess.Expression is ConstantExpression constant):
                                        {
                                            // get the value
                                            var memberValue = memberAccess.Member.GetMemberValue(constant.Value);

                                            // check if its a global value we've alreay got a query for
                                            if (context.TryGetGlobal(memberValue, out var global))
                                            {
                                                initializations.Add($"{memberName} := {global.Name}");
                                                isSubQuery = false;
                                                break;
                                            }

                                            // check if its a value returned in a previous query
                                            if (QueryObjectManager.TryGetObjectId(memberValue, out var id))
                                            {
                                                subQuery = id.SelectSubQuery(memberType);
                                                break;
                                            }
                                            
                                            // generate an insert or select based on its unique constraints.
                                            subQuery = new((info) =>
                                            {
                                                // generate an insert shape
                                                var insertShape = TranslateExpression(QueryUtils.GenerateInsertShapeExpression(memberValue, memberType), context);

                                                var exclusiveProps = QueryUtils.GetProperties(info, memberType, true);
                                                var exclusiveCondition = exclusiveProps.Any() ?
                                                    $" unless conflict on {(exclusiveProps.Count() > 1 ? $"({string.Join(", ", exclusiveProps.Select(x => $".{x.GetEdgeDBPropertyName()}"))})" : $".{exclusiveProps.First().GetEdgeDBPropertyName()}")} else (select {typeName})"
                                                    : string.Empty;

                                                return $"(insert {typeName} {{ {insertShape} }}{exclusiveCondition})";
                                            });
                                        }
                                        break;
                                    case MethodCallExpression methodCall:
                                        var parsed = TranslateExpression(methodCall, context);
                                        if(context.HasInitializationOperator)
                                        {
                                            initializations.Add($"{memberName} {parsed}");
                                            context.HasInitializationOperator = false;
                                        }
                                        else
                                            initializations.Add($"{memberName} := {parsed}");
                                        isSubQuery = false;
                                        break;
                                    default:
                                        throw new NotSupportedException($"Cannot set links with a {assignment.Expression} expression.");
                                }

                                // if its not a sub query, we can safely move onto the next member 
                                if (!isSubQuery)
                                    break;

                                // if the query was null we couldn't parse it correctly
                                if (subQuery is null)
                                    throw new NotSupportedException($"Cannot set links to the expression {assignment.Expression.NodeType}");

                                // add the sub query to the collection of child queries within the update context
                                var name = QueryUtils.GenerateRandomVariableName();
                                updateContext.ChildQueries.Add(name, subQuery);
                                initializations.Add($"{memberName} := {name}");
                                break;
                            }

                            // translate the scalar value
                            var value = TranslateExpression(assignment.Expression, context);

                            // if its null the value was a include method call, we can assume were building
                            // a shape here and just add the property name
                            if (value is null)
                                initializations.Add($"{memberName}");
                            else if (context.HasInitializationOperator) // if an initializer was added
                            {
                                initializations.Add($"{memberName} {value}");
                                context.HasInitializationOperator = false;
                            }
                            else if (context.IsShape) // if its a sub shape
                            {
                                initializations.Add($"{memberName}: {{ {value} }}");
                                context.IsShape = false;
                            }
                            else // default assignment
                                initializations.Add($"{memberName} := {value}");
                        }
                        break;
                    default:
                        throw new NotSupportedException($"Cannot translate a {binding}, please file a gh issue with your query.");
                }
            }
            
            // join the initialization list by commas
            return string.Join(", ", initializations);   
        }
    }
}
