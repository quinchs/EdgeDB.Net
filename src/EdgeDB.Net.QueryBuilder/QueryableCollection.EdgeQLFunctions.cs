using EdgeDB.QueryNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public partial class QueryableCollection<TQueryResult>
    {
        public QueryableCollection<TQueryResult> Select(object? shape = null)
        {
            AddNode<SelectNode>(new SelectContext(typeof(TQueryResult))
            {
                Shape = shape
            });
            return this;
        }

        public QueryableCollection<TQueryResult> Filter(Expression<Func<TQueryResult, bool>> condition)
        {
            if (CurrentRootNode == null)
                Select();

            AddNode<FilterNode>(new FilterContext(typeof(TQueryResult))
            {
                Expression = condition
            });

            return this;
        }

        public QueryableCollection<TQueryResult> Insert(TQueryResult value, bool returnValue = true)
        {
            var context = new InsertContext(typeof(TQueryResult))
            {
                Value = value,
                StoreAsGlobal = returnValue
            };
            
            AddNode<InsertNode>(context);

            // fix this: select isn't valid after insert which is true but the insert
            // should cause a with block to be added instead.
            if (returnValue)
            {
                AddNode<SelectNode>(new SelectContext(typeof(TQueryResult))
                {
                    SelectName = context.GlobalName
                }, false);
            }

            return this;
        }

        public QueryableCollection<TQueryResult> UnlessConflictOn(Expression<Func<TQueryResult, object?>> selector)
        {
            AddNode<UnlessConflictOnNode>(new UnlessConflictOnContext(typeof(TQueryResult))
            {
                Selector = selector
            });

            return this;
        }

        public Task<IReadOnlyCollection<TQueryResult?>> ExecuteAsync(CancellationToken token = default)
        {
            var builtQuery = Build();

            // clear the current query
            _nodes.Clear();

            return _edgedb.QueryAsync<TQueryResult>(builtQuery.Query, builtQuery.Parameters, token);
        }
    }
}
