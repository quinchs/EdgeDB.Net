using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryContext
    {
        public Type CurrentType { get; init; }

        public QueryContext(Type currentType)
        {
            CurrentType = currentType;
        }
    }
}
