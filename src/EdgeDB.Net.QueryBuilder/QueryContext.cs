using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryContext
    {
        public Type CurrentType { get; init; }

        public object? Value { get; init; }

        public QueryContext(Type currentType)
        {
            CurrentType = currentType;
        }
    }
}
