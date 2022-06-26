using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class ConstantExpressionTranslator : ExpressionTranslator<ConstantExpression>
    {
        public override string Translate(ConstantExpression expression, ExpressionContext context)
        {
            return context.IsTypeReference && expression.Value is string str ? str : QueryUtils.ParseObject(expression.Value);
        }
    }
}
