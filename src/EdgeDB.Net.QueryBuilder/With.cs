using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class With
    {
        
    }

    public class With<TQueryType> : With
    {
        private readonly QueryableCollection<TQueryType> _queryCollection;
        public With(QueryableCollection<TQueryType> queryCollection)
        {
            _queryCollection = queryCollection;
        }

        public static implicit operator With<TQueryType>(QueryableCollection<TQueryType> query) => new(query);
    }
}
