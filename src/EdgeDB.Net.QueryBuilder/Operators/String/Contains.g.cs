using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringContains : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "contains({0}, {1})";
    }
}
