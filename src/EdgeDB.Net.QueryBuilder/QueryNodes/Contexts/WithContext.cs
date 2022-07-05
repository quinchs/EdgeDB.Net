using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class WithContext : NodeContext
    {
        public List<QueryGlobal>? Values { get; init; }
        public WithContext(Type currentType) : base(currentType)
        {
        }
    }
}
