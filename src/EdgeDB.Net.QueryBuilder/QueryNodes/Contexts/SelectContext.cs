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
        public LambdaExpression? Shape { get; init; }
        public string? SelectName { get; set; }
        public bool SelectExpressional { get; set; }
        public SelectContext(Type currentType) : base(currentType)
        {
        }
    }
}
