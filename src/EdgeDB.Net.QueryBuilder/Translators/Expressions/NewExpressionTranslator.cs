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
    /// <summary>
    ///     Represents a translator for translating an expression with a constructor call.
    /// </summary>
    internal class NewExpressionTranslator : ExpressionTranslator<NewExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(NewExpression expression, ExpressionContext context)
        {
            string[] shape = new string[expression.Arguments.Count];

            // iterate over each argument to the constructor
            for(int i = 0; i != expression.Arguments.Count; i++)
            {
                // pull the member and argument out of the expression & get the edgedb name of the member
                var member = expression.Members![i];
                var arg = expression.Arguments[i];
                var edgedbName = member.GetEdgeDBPropertyName();

                // special fallthru for include
                if(arg is MethodCallExpression mcex && mcex.Method.DeclaringType == typeof(QueryContext) && mcex.Method.Name == "Include")
                {
                    shape[i] = edgedbName;
                    continue;
                }
               
                // translate the value and determine if were setting a value or referencing a value.
                string? value = TranslateExpression(arg, context.Enter(x => x.LocalScope = expression.Type)); ;
                bool isSetter = context.NodeContext.CurrentType.GetProperty(member.Name) == null || arg is MethodCallExpression;

                // add it to our shape
                shape[i] = $"{edgedbName}{(isSetter || context.IsFreeObject ? " :=" : "")} {value}";
            }
            
            // return our shape joined by commas
            return string.Join(", ", shape);
        }
    }
}
