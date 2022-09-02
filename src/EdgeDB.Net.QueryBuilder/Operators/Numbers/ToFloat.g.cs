using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class NumbersToFloat : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "to_float32({0}, {1?})";
    }
}
