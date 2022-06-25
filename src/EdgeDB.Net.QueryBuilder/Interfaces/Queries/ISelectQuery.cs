using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface ISelectQuery<TType> : IMultiCardinalityExecutable<TType>
    {
        ISelectQuery<TType> Filter(Expression<Func<TType, bool>> filter);
        ISelectQuery<TType> OrderBy(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        ISelectQuery<TType> OrderByDesending(Expression<Func<TType, object?>> propertySelector, OrderByNullPlacement? nullPlacement = null);
        ISelectQuery<TType> Offset(long offset);
        ISelectQuery<TType> Limit(long limit);
    }
}
