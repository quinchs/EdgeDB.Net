using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal static class InitializationTranslator
    {
        public static string? Translate(IDictionary<MemberInfo, Expression> expressions, ExpressionContext context)
        {
            List<string> initializations = new();
            
            foreach(var (Member, Expression) in expressions)
            {
                // get the members type and edgedb equivalent name
                var memberType = Member.GetMemberType();
                var memberName = Member.GetEdgeDBPropertyName();
                var isLink = QueryUtils.IsLink(memberType, out var isMultiLink, out var innerType);

                switch (Expression)
                {
                    case MemberExpression when isLink:
                        {
                            var disassembled = QueryUtils.DisassembleExpression(Expression).ToArray();
                            if(disassembled.Last() is ConstantExpression constant && disassembled[disassembled.Length - 2] is MemberExpression constParent)
                            {
                                // get the value
                                var memberValue = constParent.Member.GetMemberValue(constant.Value);

                                // check if its a global value we've alreay got a query for
                                if (context.TryGetGlobal(memberValue, out var global))
                                {
                                    initializations.Add($"{memberName} := {global.Name}");
                                    break;
                                }

                                // check if its a value returned in a previous query
                                if (QueryObjectManager.TryGetObjectId(memberValue, out var id))
                                {
                                    var globalName = context.GetOrAddGlobal(id, id.SelectSubQuery(memberType));
                                    initializations.Add($"{memberName} := {globalName}");
                                    break;
                                }

                                // generate an insert or select based on its unique constraints.
                                var name = QueryUtils.GenerateRandomVariableName();
                                context.SetGlobal(name, new SubQuery((info) =>
                                {
                                    // generate an insert shape
                                    var insertShape = ExpressionTranslator.ContextualTranslate(QueryUtils.GenerateInsertShapeExpression(memberValue, memberType), context);

                                    var exclusiveProps = QueryUtils.GetProperties(info, memberType, true);
                                    var exclusiveCondition = exclusiveProps.Any() ?
                                        $" unless conflict on {(exclusiveProps.Count() > 1 ? $"({string.Join(", ", exclusiveProps.Select(x => $".{x.GetEdgeDBPropertyName()}"))})" : $".{exclusiveProps.First().GetEdgeDBPropertyName()}")} else (select {memberType.GetEdgeDBTypeName()})"
                                        : string.Empty;

                                    return $"(insert {memberType.GetEdgeDBTypeName()} {{ {insertShape} }}{exclusiveCondition})";
                                }), null);
                                initializations.Add($"{memberName} := {name}");
                            }
                        }
                        break;
                    case MemberInitExpression or NewExpression:
                        {
                            var name = QueryUtils.GenerateRandomVariableName();
                            var expression = Expression;
                            context.SetGlobal(name, new SubQuery((info) =>
                            {
                                // generate an insert shape
                                var insertShape = ExpressionTranslator.ContextualTranslate(expression, context);

                                var exclusiveProps = QueryUtils.GetProperties(info, memberType, true);
                                var exclusiveCondition = exclusiveProps.Any() ?
                                    $" unless conflict on {(exclusiveProps.Count() > 1 ? $"({string.Join(", ", exclusiveProps.Select(x => $".{x.GetEdgeDBPropertyName()}"))})" : $".{exclusiveProps.First().GetEdgeDBPropertyName()}")} else (select {memberType.GetEdgeDBTypeName()})"
                                    : string.Empty;

                                return $"(insert {memberType.GetEdgeDBTypeName()} {{ {insertShape} }}{exclusiveCondition})";
                            }), null);
                            initializations.Add($"{memberName} := {name}");
                        }
                        break;
                    default:
                        {
                            // translate the value and determine if were setting a value or referencing a value.
                            var newContext = context.Enter(x =>
                            {
                                x.LocalScope = memberType;
                                x.IsShape = false;
                            });
                            string? value = ExpressionTranslator.ContextualTranslate(Expression, newContext);
                            bool isSetter = context.NodeContext is InsertContext || context.NodeContext.CurrentType.GetProperty(Member.Name) == null || Expression is MethodCallExpression;

                            // add it to our shape
                            if (value is null) // include
                                initializations.Add(memberName);
                            else if (newContext.IsShape) // includelink
                                initializations.Add($"{memberName}: {{ {value} }}");
                            else
                                initializations.Add($"{memberName}{(isSetter || context.IsFreeObject ? " :=" : "")} {value}");
                        }
                        break;
                }
            }

            context.IsShape = true;
            return string.Join(", ", initializations);
        }
    }
}
