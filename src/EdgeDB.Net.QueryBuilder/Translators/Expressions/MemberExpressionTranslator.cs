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
            // deconstruct the member access tree.
            var deconstructed = ExpressionUtils.DisassembleExpression(expression).ToArray();
            
            // if the resolute expression is a constant expression, assume
            // were in a set-like context and add it as a variable.
            if (deconstructed.LastOrDefault() is ConstantExpression constant)
            {
                // walk thru the reference tree, you can imagine this as a variac pointer resolution.
                object? refHolder = constant.Value;
                
                for(int i = deconstructed.Length - 2; i >= 0; i--)
                {
                    // if the deconstructed node is not a member expression, we have something fishy...
                    if (deconstructed[i] is not MemberExpression memberExp)
                        throw new InvalidOperationException("Member tree does not contain all members. this is a bug, please file a github issue with the query that caused this exception.");

                    // gain the new reference holder to the value we're after
                    refHolder = memberExp.Member.GetMemberValue(refHolder);
                }

                // at this point, 'refHolder' is now a direct reference to the property the expression resolves to,
                // we can add this as our variable.
                var varName = context.AddVariable(refHolder);
                
                if (!EdgeDBTypeUtils.TryGetScalarType(expression.Type, out var type))
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
