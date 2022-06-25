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
