using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    internal class MemberInitExpressionTranslator : ExpressionTranslator<MemberInitExpression>
    {
        public override string Translate(MemberInitExpression expression, ExpressionContext context)
        {
            List<string> initializations = new();

            foreach(var binding in expression.Bindings)
            {
                switch (binding)
                {
                    case MemberAssignment assignment:
                        {
                            var value = TranslateExpression(assignment.Expression, context);
                            initializations.Add($"{assignment.Member.GetEdgeDBPropertyName()} := {value}");
                        }
                        break;
                }
            }
            
            return string.Join(", ", initializations);   
        }
    }
}
