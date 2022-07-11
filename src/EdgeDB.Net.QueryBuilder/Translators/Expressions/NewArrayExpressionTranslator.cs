using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class NewArrayExpressionTranslator : ExpressionTranslator<NewArrayExpression>
    {
        public override string? Translate(NewArrayExpression expression, ExpressionContext context)
        {
            return $"{{ {string.Join(", ", expression.Expressions.Select(x => TranslateExpression(x, context)))} }}";
        }
    }
}
