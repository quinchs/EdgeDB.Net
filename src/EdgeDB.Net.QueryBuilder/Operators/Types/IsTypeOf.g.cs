using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIsTypeOf : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "{0} is typeof {1}";
    }
}
