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
    internal class MemberInitExpressionTranslator : ExpressionTranslator<MemberInitExpression>
    {
        public override string? Translate(MemberInitExpression expression, ExpressionContext context)
        {
            List<string> initializations = new();

            foreach(var binding in expression.Bindings)
            {
                switch (binding)
                {
                    case MemberAssignment assignment:
                        {
                            var memberType = assignment.Member.GetMemberType();
                            var memberName = assignment.Member.GetEdgeDBPropertyName();

                            if (TypeBuilder.IsValidObjectType(memberType))
                            {
                                var typeName = memberType.GetEdgeDBTypeName();

                                // at this point we're setting a link property here, if we're in a select node and its a shape def
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
                                
                                if (context.NodeContext is not UpdateContext updateContext)
                                    throw new NotSupportedException("Cannot set links with the current node");

                                SubQuery? subQuery = null;
                                bool isSubQuery = true;

                                switch (assignment.Expression)
                                {
                                    case MemberExpression memberAccess when (memberAccess.Expression is ConstantExpression constant):
                                        {
                                            // get the value
                                            var memberValue = memberAccess.Member.GetMemberValue(constant.Value);

                                            // check if its a value we've seen before
                                            if (context.TryGetGlobal(memberValue, out var global))
                                            {
                                                initializations.Add($"{memberName} := {global.Name}");
                                                isSubQuery = false;
                                                break;
                                            }
                                            
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
                                    default:
                                        throw new NotSupportedException("Cannot set links with the current expression");
                                }

                                if (!isSubQuery)
                                    break;

                                if (subQuery is null)
                                    throw new NotSupportedException($"Cannot set links to the expression {assignment.Expression.NodeType}");

                                var name = QueryUtils.GenerateRandomVariableName();
                                updateContext.ChildQueries.Add(name, subQuery);
                                initializations.Add($"{memberName} := {name}");
                                break;
                            }

                            var value = TranslateExpression(assignment.Expression, context);


                            if (value is null)
                                initializations.Add($"{memberName}");
                            else if (context.IsShape)
                            {
                                initializations.Add($"{memberName}: {{ {value} }}");
                                context.IsShape = false;
                            }
                            else
                                initializations.Add($"{memberName} := {value}");

                        }
                        break;
                }
            }
            
            return string.Join(", ", initializations);   
        }
    }
}
