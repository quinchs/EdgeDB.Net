using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal abstract class ExpressionTranslator<TExpression> : ExpressionTranslator
        where TExpression : Expression
    {
        public abstract string Translate(TExpression expression, ExpressionContext context);

        public override string Translate(Expression expression, ExpressionContext context)
        {
            return Translate((TExpression)expression, context);
        }
    }

    internal abstract class ExpressionTranslator
    {
        private static readonly Dictionary<Type, ExpressionTranslator> _translators = new();
        private static readonly IEdgeQLOperator[] _operators;
        private static readonly Dictionary<ExpressionType, IEdgeQLOperator> _expressionOperators;

        static ExpressionTranslator()
        {
            var types = Assembly.GetExecutingAssembly().DefinedTypes;
            // load current translators
            var translators = types.Where(x => x.BaseType?.Name == "ExpressionTranslator`1");

            foreach(var translator in translators)
            {
                _translators[translator.BaseType!.GenericTypeArguments[0]] = (ExpressionTranslator)Activator.CreateInstance(translator)!;
            }

            // load operators
            _operators = types.Where(x => x.ImplementedInterfaces.Any(x => x == typeof(IEdgeQLOperator))).Select(x => (IEdgeQLOperator)Activator.CreateInstance(x)!).ToArray();

            // set the expression operators
            _expressionOperators = _operators.Where(x => x.Expression is not null).DistinctBy(x => x.Expression).ToDictionary(x => (ExpressionType)x.Expression!, x => x);
        }

        protected static bool TryGetExpressionOperator(ExpressionType type, [MaybeNullWhen(false)] out IEdgeQLOperator edgeqlOperator)
            => _expressionOperators.TryGetValue(type, out edgeqlOperator);


        public abstract string Translate(Expression expression, ExpressionContext context);

        public static string Translate<TInnerExpression>(Expression<TInnerExpression> expression)
            => Translate(expression);

        public static string Translate(LambdaExpression expression, IDictionary<string, object?> queryArguments, NodeContext nodeContext)
        {
            var context = new ExpressionContext(nodeContext, expression, queryArguments);
            return TranslateExpression(expression.Body, context);
        }

        protected static string TranslateExpression(Expression expression, ExpressionContext context)
        {
            var expType = expression.GetType();
            while (!expType.IsPublic)
                expType = expType.BaseType!;  

            if (_translators.TryGetValue(expType, out var translator))
                return translator.Translate(expression, context);

            throw new Exception("AAAA");
        }
    }
}
