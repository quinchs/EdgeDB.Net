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
        public override string? Translate(BinaryExpression expression, ExpressionContext context)
        {
            var left = TranslateExpression(expression.Left, context);
            var right = TranslateExpression(expression.Right, context);

            // special case for exists keyword
            if ((expression.Right is ConstantExpression rightConst && rightConst.Value is null ||
               expression.Left is ConstantExpression leftConst && leftConst.Value is null) &&
               expression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual)
            {
                return $"{(expression.NodeType is ExpressionType.Equal ? "not exists" : "exists")} {(right == "{}" ? left : right)}";
            }

            if (!TryGetExpressionOperator(expression.NodeType, out var op))
                throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");

            return op.Build(left, right);
        }
    }
}
