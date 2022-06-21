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

        public ExpressionContext(LambdaExpression rootExpression)
        {
            RootExpression = rootExpression;

            Parameters = rootExpression.Parameters.ToDictionary(x => x.Name!, x => x.Type);
        }
    }
}
