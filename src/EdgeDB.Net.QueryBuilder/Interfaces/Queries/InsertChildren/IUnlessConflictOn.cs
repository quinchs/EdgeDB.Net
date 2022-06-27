using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces
{
    public interface IUnlessConflictOn<TType> : ISingleCardinalityExecutable<TType>
    {
        ISingleCardinalityExecutable<TType> ElseReturn();
        IMultiCardinalityExecutable<TType> Else(Func<IQueryBuilder<TType>, IMultiCardinalityExecutable<TType>> elseQuery);
        ISingleCardinalityExecutable<TType> Else(Func<IQueryBuilder<TType>, ISingleCardinalityExecutable<TType>> elseQuery);
        IQueryBuilder<object?> Else<TQueryBuilder>(TQueryBuilder elseQuery)
            where TQueryBuilder : IQueryBuilder;
    }
}
