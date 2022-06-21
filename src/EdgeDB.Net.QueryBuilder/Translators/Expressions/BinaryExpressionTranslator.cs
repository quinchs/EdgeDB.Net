using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class BinaryExpressionTranslator : ExpressionTranslator<BinaryExpression>
    {
        public override string Translate(BinaryExpression expression, ExpressionContext context)
        {
            var left = TranslateExpression(expression, context);
            var right = TranslateExpression(expression, context);

            if (!TryGetExpressionOperator(expression.NodeType, out var op))
                throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");

            return op.Build(left, right);
        }
    }
}
