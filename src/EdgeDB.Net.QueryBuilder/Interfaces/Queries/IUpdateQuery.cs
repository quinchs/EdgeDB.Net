using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface IUpdateQuery<TType> : IMultiCardinalityExecutable<TType>
    {
        IMultiCardinalityExecutable<TType> Filter(Expression<Func<TType, bool>> filter);
    }
}
