using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class SelectContext : NodeContext
    {
        public Expression<Func<object>>? Shape { get; init; }
        public string? SelectName { get; init; }
        public SelectContext(Type currentType) : base(currentType)
        {
        }
    }
}
