using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Translators.Expressions
{
    /// <summary>
    ///     Represents a translator for translating an expression with a unary operator.
    /// </summary>
    internal class UnaryExpressionTranslator : ExpressionTranslator<UnaryExpression>
    {
        /// <inheritdoc/>
        public override string? Translate(UnaryExpression expression, ExpressionContext context)
        {
            switch (expression.NodeType)
            {
                // quote expressions are literal funcs (im pretty sure), so we can just
                // directly translate them and return the result.
                case ExpressionType.Quote when expression.Operand is LambdaExpression lambda:
                    return TranslateExpression(lambda.Body, context.Enter(x => x.StringWithoutQuotes = false));

                // convert is a type change, so we translate the dotnet form '(type)value' to '<type>value'
                case ExpressionType.Convert:
                    {
                        var value = TranslateExpression(expression.Operand, context);
                        
                        if (value is null)
                            return null; // nullable converters for include, ex Guid? -> Guid

                        // dotnet nullable check
                        if (ReflectionUtils.IsInstanceOfGenericType(typeof(Nullable<>), expression.Type) && 
                            expression.Type.GenericTypeArguments[0] == expression.Operand.Type)
                        {
                            // no need to cast in edgedb, return the value   
                            return value;
                        }   

                        var type = PacketSerializer.GetEdgeQLType(expression.Type) ?? expression.Type.GetEdgeDBTypeName();
                       
                        return $"<{type}>{value}";
                    }

                // default case attempts to get an IEdgeQLOperator for the given
                // node type, and uses that to translate the expression.
                default:
                    if (!TryGetExpressionOperator(expression.NodeType, out var op))
                        throw new NotSupportedException($"Failed to find operator for node type {expression.NodeType}");
                    return op.Build(TranslateExpression(expression.Operand, context));
            }

            throw new NotSupportedException($"Failed to find converter for {expression.NodeType}!");
        }
    }
}
