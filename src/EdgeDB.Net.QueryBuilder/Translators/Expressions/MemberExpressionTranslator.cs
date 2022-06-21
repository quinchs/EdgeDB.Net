using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class MemberExpressionTranslator : ExpressionTranslator<MemberExpression>
    {
        public override string Translate(MemberExpression expression, ExpressionContext context)
        {
            return ParseMemberExpression(expression, expression.Expression is not ParameterExpression);
        }

        private static string ParseMemberExpression(MemberExpression expression, bool includeParameter = true)
        {
            List<string?> tree = new();

            tree.Add(expression.Member.GetEdgeDBPropertyName());
            if (expression.Expression is MemberExpression innerExp)
                tree.Add(ParseMemberExpression(innerExp));
            if (expression.Expression is ParameterExpression param)
                tree.Add(includeParameter ? param.Name : string.Empty);

            tree.Reverse();
            return string.Join('.', tree);
        }
    }
}
