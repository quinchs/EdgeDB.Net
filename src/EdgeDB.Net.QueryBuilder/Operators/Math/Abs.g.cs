using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class MathAbs : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "math::abs({0})";
    }
}
