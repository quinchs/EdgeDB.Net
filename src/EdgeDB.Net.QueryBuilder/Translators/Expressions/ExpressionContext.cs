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
    internal class ExpressionContext
    {
        public NodeContext NodeContext { get; }
        public LambdaExpression RootExpression { get; }
        public Dictionary<string, Type> Parameters { get; }
        private readonly IDictionary<string, object?> _queryObjects;
        private readonly List<QueryGlobal> _globals;
        public bool StringWithoutQuotes { get; set; }
        public Type? LocalScope { get; set; }
        public bool IsShape { get; set; }


        public ExpressionContext(NodeContext context, LambdaExpression rootExpression, 
            IDictionary<string, object?> queryArguments, List<QueryGlobal> globals)
        {
            RootExpression = rootExpression;
            _queryObjects = queryArguments;
            NodeContext = context;
            _globals = globals;

            Parameters = rootExpression.Parameters.ToDictionary(x => x.Name!, x => x.Type);
        }

        public string AddVariable(object? value)
        {
            var name = QueryUtils.GenerateRandomVariableName();
            _queryObjects[name] = value;
            return name;
        }

        public bool TryGetGlobal(object? value, [MaybeNullWhen(false)]out QueryGlobal global)
        {
            global = _globals.FirstOrDefault(x => x.Reference == value);
            return global != null;
        }

        public ExpressionContext Enter(Action<ExpressionContext> func)
        {
            var exp = (ExpressionContext)MemberwiseClone();
            func(exp);
            return exp;
        }
    }
}
