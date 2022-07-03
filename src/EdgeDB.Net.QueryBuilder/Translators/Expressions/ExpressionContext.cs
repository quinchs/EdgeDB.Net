using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class ExpressionContext
    {
        public NodeContext NodeContext { get; }
        public LambdaExpression RootExpression { get; }
        public Dictionary<string, Type> Parameters { get; }
        private readonly IDictionary<string, object?> _queryObjects;

        public bool StringWithoutQuotes { get; set; }
        public Type? LocalScope { get; set; }
        public bool IsShape { get; set; }


        public ExpressionContext(NodeContext context, LambdaExpression rootExpression, IDictionary<string, object?> queryArguments)
        {
            RootExpression = rootExpression;
            _queryObjects = queryArguments;
            NodeContext = context;

            Parameters = rootExpression.Parameters.ToDictionary(x => x.Name!, x => x.Type);
        }

        public string AddVariable(object? value)
        {
            var name = QueryUtils.GenerateRandomVariableName();
            _queryObjects[name] = value;
            return name;
        }

        public ExpressionContext Enter(Action<ExpressionContext> func)
        {
            var exp = (ExpressionContext)MemberwiseClone();
            func(exp);
            return exp;
        }
    }
}
