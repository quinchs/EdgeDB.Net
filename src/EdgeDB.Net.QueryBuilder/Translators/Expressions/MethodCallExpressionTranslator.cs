using EdgeDB.Operators;
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
        public override string Translate(MethodCallExpression expression, ExpressionContext context)
        {
            // special case for local
            if(expression.Method.DeclaringType == typeof(QueryContext) && expression.Method.Name == "Local")
            {
                // check arg scope
                var rawArg = TranslateExpression(expression.Arguments[0], context.Enter(x => x.IsTypeReference = true));
                var rawPath = rawArg.Split('.');
                string[] parsedPath = new string[rawPath.Length];

                for(int i = 0; i != rawPath.Length; i++)
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

            // check if the method has an 'EquivalentOperator' attribute
            var edgeqlOperator = expression.Method.GetCustomAttribute<EquivalentOperator>()?.Operator;

            if (edgeqlOperator != null)
            {
                // parse the parameters 
                var argsArray = new object[expression.Arguments.Count];
                for(int i = 0; i != argsArray.Length; i++)
                    argsArray[i] = TranslateExpression(expression.Arguments[i], context);
                return edgeqlOperator.Build(argsArray);
            }

            // check if its a known method 
            if(EdgeQL.FunctionOperators.TryGetValue($"{expression.Method.DeclaringType?.Name}.{expression.Method.Name}", out edgeqlOperator))
            {
                var args = (expression.Object != null ? new string[] { TranslateExpression(expression.Object, context) } : Array.Empty<string>()).Concat(expression.Arguments.Select(x => TranslateExpression(x, context))); 
                return edgeqlOperator.Build(args.ToArray());
            }

            throw new Exception($"Couldn't find translator for {expression.Method.Name}");
        }
    }
}
