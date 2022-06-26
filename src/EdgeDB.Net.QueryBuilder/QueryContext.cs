using EdgeDB.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public sealed class QueryContext
    {
        [EquivalentOperator(typeof(VariablesReference))]
        public TType Global<TType>(string name)
            => default!;

        [EquivalentOperator(typeof(LocalReference))]
        public TType Local<TType>(string name)
            => default!;

        public TType Include<TType>()
            => default!;
    }
}
