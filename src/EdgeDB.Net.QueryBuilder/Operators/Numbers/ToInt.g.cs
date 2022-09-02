using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToInt : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "to_int32({0}, {1?})";
    }
}
