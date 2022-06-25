using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class UpdateContext : NodeContext
    {
        public string? UpdateName { get; init; }
        public LambdaExpression? UpdateExpression { get; init; }

        public UpdateContext(Type currentType) : base(currentType)
        {
        }
    }
}
