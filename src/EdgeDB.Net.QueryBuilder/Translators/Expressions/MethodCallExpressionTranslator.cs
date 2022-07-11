﻿using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class MethodCallExpressionTranslator : ExpressionTranslator<MethodCallExpression>
    {
        public override string? Translate(MethodCallExpression expression, ExpressionContext context)
        {
            // special case for local
            if (expression.Method.DeclaringType == typeof(QueryContext))
            {
                switch (expression.Method.Name)
                {
                    case nameof(QueryContext.Local):
                        {
                            // check arg scope
                            var rawArg = TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true));
                            var rawPath = rawArg.Split('.');
                            string[] parsedPath = new string[rawPath.Length];

                            for (int i = 0; i != rawPath.Length; i++)
                            {
                                var prop = (MemberInfo?)context.LocalScope?.GetProperty(rawPath[i]) ??
                                    context.LocalScope?.GetField(rawPath[i]) ??
                                    (MemberInfo?)context.NodeContext.CurrentType.GetProperty(rawPath[i]) ??
                                    context.NodeContext.CurrentType.GetField(rawPath[i]);

                                if (prop is null)
                                    throw new InvalidOperationException($"The property \"{rawPath[i]}\" within \"{rawArg}\" is out of scope");

                                parsedPath[i] = prop.GetEdgeDBPropertyName();
                            }

                            return $".{string.Join('.', parsedPath)}";
                        }
                    case nameof(QueryContext.UnsafeLocal):
                        {
                            return $".{TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true))}";
                        }
                    case nameof(QueryContext.Include):
                        {
                            // do nothing here
                            return null;
                        }
                    case nameof(QueryContext.IncludeLink) or nameof(QueryContext.IncludeMultiLink):
                        {
                            // parse the inner shape
                            var shape = TranslateExpression(expression.Arguments[0], context);
                            context.IsShape = true;
                            return shape;
                        }
                    case nameof(QueryContext.Raw):
                        {
                            return TranslateExpression(expression.Arguments[0], context.Enter(x => x.StringWithoutQuotes = true));
                        }
                    case nameof(QueryContext.BackLink):
                        {
                            var isRawPropertyName = expression.Arguments[0].Type == typeof(string);
                            var hasShape = !isRawPropertyName && expression.Arguments.Count > 1;
                            var property = TranslateExpression(expression.Arguments[0],
                                isRawPropertyName
                                    ? context.Enter(x => x.StringWithoutQuotes = true) 
                                    : context.Enter(x => x.IncludeSelfReference = false));
                            
                            var backlink = $".<{property}";

                            if (!isRawPropertyName)
                                backlink += $"[is {expression.Method.GetGenericArguments()[0].GetEdgeDBTypeName()}]";

                            if (hasShape)
                                backlink += $"{{{TranslateExpression(expression.Arguments[1], context)}}}";

                            return backlink;
                        }
                    default:
                        throw new NotImplementedException($"{expression.Method.Name} does not have an implementation. This is a bug, please file a github issue with your query to reproduce this exception.");

                }
            }

            // check if the method has an 'EquivalentOperator' attribute
            var edgeqlOperator = expression.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;

            if (edgeqlOperator != null)
            {
                // parse the parameters 
                var argsArray = new object[expression.Arguments.Count];
                var parameters = expression.Method.GetParameters();
                for (int i = 0; i != argsArray.Length; i++)
                {
                    var arg = expression.Arguments[i];
                    if (parameters[i].ParameterType.IsAssignableTo(typeof(IQueryBuilder)))
                    {
                        // compile and run the value
                        var builder = (IQueryBuilder)Expression.Lambda(arg).Compile().DynamicInvoke()!;

                        // TODO: support variables & globals
                        var result = builder.BuildWithGlobals();
                        if (result.Globals?.Any() ?? false)
                            throw new NotSupportedException("Cannot use queries with parameters or globals within a sub-query expression");

                        if (result.Parameters is not null)
                            foreach (var parameter in result.Parameters)
                                context.SetVariable(parameter.Key, parameter.Value);

                        argsArray[i] = context.GetOrAddGlobal(null, new SubQuery($"({result.Query})"));
                    }
                    else
                        argsArray[i] = TranslateExpression(arg, context);
                }

                context.HasInitializationOperator = edgeqlOperator switch
                {
                    LinksAddLink or LinksRemoveLink => true,
                    _ => false
                };

                return edgeqlOperator.Build(argsArray);
            }

            // check if its a known method 
            if (EdgeQL.FunctionOperators.TryGetValue($"{expression.Method.DeclaringType?.Name}.{expression.Method.Name}", out edgeqlOperator))
            {
                var args = (expression.Object != null ? new string[] { TranslateExpression(expression.Object, context) } : Array.Empty<string>()).Concat(expression.Arguments.Select(x => TranslateExpression(x, context)));
                return edgeqlOperator.Build(args.ToArray());
            }

            throw new Exception($"Couldn't find translator for {expression.Method.Name}");
        }
    }
}