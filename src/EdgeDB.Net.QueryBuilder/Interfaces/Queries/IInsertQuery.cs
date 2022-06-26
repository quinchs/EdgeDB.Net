using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface IInsertQuery<TType> : ISingleCardinalityExecutable<TType>
    {
        IUnlessConflictOn<TType> UnlessConflictOn(Expression<Func<TType, object?>> propertySelector);
        IUnlessConflictOn<TType> UnlessConflictOn(Expression<Func<QueryContext, TType, object?>> propertySelector);
    }
}
