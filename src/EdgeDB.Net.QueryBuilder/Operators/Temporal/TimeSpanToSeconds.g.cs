using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalTimeSpanToSeconds : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "std::duration_to_seconds({0})";
    }
}
