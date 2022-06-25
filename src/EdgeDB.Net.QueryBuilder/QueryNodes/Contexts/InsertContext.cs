using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class InsertContext : NodeContext
    {
        public object? Value { get; init; }
        public InsertContext(Type currentType) : base(currentType)
        {
        }
    }
}
