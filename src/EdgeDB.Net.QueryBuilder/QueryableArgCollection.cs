using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryableArgCollection<TTuple, TQueryResult> : QueryableCollection<TQueryResult>
        where TTuple : ITuple
    {
        private readonly TTuple _tuple;

        internal QueryableArgCollection(IEdgeDBQueryable edgedb, TTuple tuple) : base(edgedb)
        {
            _tuple = tuple;
        }

        
    }
}
