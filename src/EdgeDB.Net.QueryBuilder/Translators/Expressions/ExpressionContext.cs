using EdgeDB.QueryNodes;
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
        public List<Expression> ExpressionTree { get; set; } = new();

        public NodeContext NodeContext { get; }
        public LambdaExpression RootExpression { get; }
        public Dictionary<string, Type> Parameters { get; }
        private readonly IDictionary<string, object?> _queryObjects;
        private readonly List<QueryGlobal> _globals;
        public bool StringWithoutQuotes { get; set; }
        public Type? LocalScope { get; set; }
        public bool IsShape { get; set; }
        public bool HasInitializationOperator { get; set; }
        public bool IncludeSelfReference { get; set; } = true;
        public bool IsFreeObject
            => NodeContext is SelectContext selectContext && selectContext.IsFreeObject;

        public ExpressionContext(NodeContext context, LambdaExpression rootExpression, 
            IDictionary<string, object?> queryArguments, List<QueryGlobal> globals)
        {
            ExpressionTree.Add(rootExpression);
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

        public void SetVariable(string name, object? value)
            => _queryObjects[name] = value;

        public bool TryGetGlobal(object? value, [MaybeNullWhen(false)]out QueryGlobal global)
        {
            global = _globals.FirstOrDefault(x => x.Reference == value);
            return global != null;
        }

        public string GetOrAddGlobal(object? reference, object? value)
        {
            if(reference is not null)
            {
                var global = _globals.FirstOrDefault(x => x.Value == value);
                if (global != null)
                    return global.Name;
            }
           
            var name = QueryUtils.GenerateRandomVariableName();
            SetGlobal(name, value, reference);
            return name;
        }

        public void SetGlobal(string name, object? value, object? reference)
        {
            var global = new QueryGlobal(name, value, reference);
            _globals.Add(global);
        }

        public ExpressionContext Enter(Action<ExpressionContext> func)
        {
            var exp = (ExpressionContext)MemberwiseClone();
            func(exp);
            exp.ExpressionTree = ExpressionTree.ToList(); // creates a new instance as we dont want to copy ref of this contexts tree.
            return exp;
        }
    }
}
