using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class SubQuery
    {
        public string Query { get; init; }

        public SubQuery(string query)
        {
            Query = query;
        }
    }
}
