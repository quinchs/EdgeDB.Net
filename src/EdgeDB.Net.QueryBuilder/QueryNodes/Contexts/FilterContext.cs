using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class FilterContext : QueryContext
    {
        public LambdaExpression Expression { get; init; }

        public FilterContext(Type currentType, LambdaExpression expression) : base(currentType)
        {
            Expression = expression;
        }
    }
}
