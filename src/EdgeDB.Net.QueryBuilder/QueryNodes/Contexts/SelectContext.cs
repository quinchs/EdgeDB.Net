using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class SelectContext : QueryContext
    {
        public object? Shape { get; init; }

        public SelectContext(Type currentType) : base(currentType)
        {
        }
    }
}
