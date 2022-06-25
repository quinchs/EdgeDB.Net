using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class NodeContext
    {
        public bool SetAsGlobal { get; init; }
        public string? GlobalName { get; init; }
        public Type CurrentType { get; init; }

        public NodeContext(Type currentType)
        {
            CurrentType = currentType;
        }
    }
}
