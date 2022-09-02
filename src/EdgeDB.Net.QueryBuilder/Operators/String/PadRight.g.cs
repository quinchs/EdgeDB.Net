using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringPadRight : IEdgeQLOperator
    {
        public ExpressionType? Expression => null;
        public string EdgeQLOperator => "str_pad_end({0}, {1}, {2?})";
    }
}
