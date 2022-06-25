using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class ExpressionContext
    {
        public LambdaExpression RootExpression { get; }
        public Dictionary<string, Type> Parameters { get; }
        private readonly IDictionary<string, object?> _queryObjects;

        public ExpressionContext(LambdaExpression rootExpression, IDictionary<string, object?> queryArguments)
        {
            RootExpression = rootExpression;
            _queryObjects = queryArguments;

            Parameters = rootExpression.Parameters.ToDictionary(x => x.Name!, x => x.Type);
        }

        public string AddVariable(object? value)
        {
            var name = QueryUtils.GenerateRandomVariableName();
            _queryObjects[name] = value;
            return name;
        }
    }
}
