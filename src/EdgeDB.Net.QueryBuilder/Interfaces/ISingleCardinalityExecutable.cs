using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    public interface ISingleCardinalityExecutable<TType> : IQueryBuilder
    {
        Task<TType?> ExecuteAsync(IEdgeDBQueryable edgedb, CancellationToken token = default);    
    }
}
