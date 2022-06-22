using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class UnlessConflictOnContext : QueryContext
    {
        public LambdaExpression? Selector { get; init; }

        public UnlessConflictOnContext(Type currentType) : base(currentType) { }
    }
}
