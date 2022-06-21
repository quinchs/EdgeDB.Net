using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class OrderByContext : QueryContext
    {
        public OrderByDirection Direction { get; init; }
        public EmptyPlacement? EmptyPlacement { get; init; }
        public LambdaExpression Expression { get; init; }

        public OrderByContext(Type currentType, LambdaExpression expression) : base(currentType)
        {
            Expression = expression;
        }
    }

    internal enum OrderByDirection
    {
        Ascending,
        Descending,
    }

    internal enum EmptyPlacement
    {
        First,
        Last,
    }
}
