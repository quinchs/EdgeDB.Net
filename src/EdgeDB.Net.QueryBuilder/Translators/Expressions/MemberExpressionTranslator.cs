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
    ///     Represents a translator for translating an expression accessing a field or property.
    /// </summary>
    internal class MemberExpressionTranslator : ExpressionTranslator<MemberExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(MemberExpression expression, ExpressionContext context)
        {
            // if the inner expression is a constant value we can get, assume
            // were in a set-like context and add it as a variable.
            if (expression.Expression is ConstantExpression constant)
            {
                object? value = expression.Member.GetMemberValue(constant.Value);

                var varName = context.AddVariable(value);
                
                if (!QueryUtils.TryGetScalarType(expression.Type, out var type))
                    throw new NotSupportedException($"Cannot use {expression.Type} as no edgeql equivalent can be found");

                return $"<{type}>${varName}";
            }
            
            // assume were in a access-like context and reference it in edgeql.
            return ParseMemberExpression(expression, expression.Expression is not ParameterExpression, context.IncludeSelfReference);
        }

        /// <summary>
        ///     Parses a given member expression into a member access list.
        /// </summary>
        /// <param name="expression">The expression to parse.</param>
        /// <param name="includeParameter">Whether or not to include the referenced parameter name.</param>
        /// <param name="includeSelfReference">Whether or not to include a self reference, ex: '.'.</param>
        /// <returns></returns>
        private static string ParseMemberExpression(MemberExpression expression, bool includeParameter = true, bool includeSelfReference = true)
        {
            List<string?> tree = new()
            {
                expression.Member.GetEdgeDBPropertyName()
            };
            
            if (expression.Expression is MemberExpression innerExp)
                tree.Add(ParseMemberExpression(innerExp));
            if (expression.Expression is ParameterExpression param)
                if(includeSelfReference)
                    tree.Add(includeParameter ? param.Name : string.Empty);

            tree.Reverse();
            return string.Join('.', tree);
        }
    }
}
