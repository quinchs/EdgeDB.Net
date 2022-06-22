using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class EdgeDBExtensions
    {
        public static QueryableCollection<TType> GetCollection<TType>(this IEdgeDBQueryable edgedb)
        {
            return new QueryableCollection<TType>(edgedb);
        }
    }
}
