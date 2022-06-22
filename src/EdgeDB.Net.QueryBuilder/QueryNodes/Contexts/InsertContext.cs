using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.QueryNodes
{
    internal class InsertContext : QueryContext
    {
        public object? Value { get; init; }
        public bool StoreAsGlobal { get; init; }
        public string? GlobalName { get; set; }
        public InsertContext(Type currentType) : base(currentType)
        {
        }
    }
}
