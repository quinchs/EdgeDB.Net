﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Interfaces.Queries
{
    public interface IGroupQuery<TType> : IMultiCardinalityExecutable<TType>
    {
        
    }
}
