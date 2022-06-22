using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class WithContext : QueryContext
    {
        public Dictionary<string, object?>? Values { get; init; }
        public WithContext(Type currentType) : base(currentType)
        {
        }
    }
}
