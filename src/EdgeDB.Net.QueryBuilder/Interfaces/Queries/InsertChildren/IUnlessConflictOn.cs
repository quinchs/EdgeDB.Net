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
        // TODO: Else expression
    }
}
