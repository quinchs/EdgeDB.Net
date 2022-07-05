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
    internal class NewExpressionTranslator : ExpressionTranslator<NewExpression>
    {
        public override string? Translate(NewExpression expression, ExpressionContext context)
        {
            string[] shape = new string[expression.Arguments.Count];

            for(int i = 0; i != expression.Arguments.Count; i++)
            {
                var member = expression.Members![i];
                var arg = expression.Arguments[i];
                var edgedbName = member.GetEdgeDBPropertyName();

                if(arg is MethodCallExpression mcex && mcex.Method.DeclaringType == typeof(QueryContext) && mcex.Method.Name == "Include")
                {
                    shape[i] = edgedbName;
                    continue;
                }

                var type = member switch
                {
                    PropertyInfo property => property.PropertyType,
                    FieldInfo field => field.FieldType,
                    _ => throw new NotSupportedException($"Cannot use {member} in anonomous shape selects")
                };

                string? value = null;
                bool isSetter = true;
                // local reference, verify in the anon obj
                if (arg is MethodCallExpression mcall)
                {
                    value = TranslateExpression(mcall, context.Enter(x => x.LocalScope = expression.Type));
                }
                else
                {
                    isSetter = context.NodeContext.CurrentType.GetProperty(member.Name) == null;
                    value = TranslateExpression(expression.Arguments[i], context.Enter(x => x.LocalScope = expression.Type));
                }
                
                shape[i] = $"{edgedbName}{(isSetter ? " :=" : "")} {value}";
            }
            
            return string.Join(", ", shape);
        }
    }
}
