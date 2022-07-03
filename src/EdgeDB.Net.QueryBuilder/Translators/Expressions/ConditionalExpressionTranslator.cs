using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class ConditionalExpressionTranslator : ExpressionTranslator<ConditionalExpression>
    {
        public override string? Translate(ConditionalExpression expression, ExpressionContext context)
        {
            var condition = TranslateExpression(expression.Test, context);
            var ifTrue = TranslateExpression(expression.IfTrue, context);
            var ifFalse = TranslateExpression(expression.IfFalse, context);

            
            return $"{ifTrue} if {condition} else {ifFalse}";
        }
    }
}
