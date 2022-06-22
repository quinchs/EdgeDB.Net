using EdgeDB.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     The general purpouse of this class is to manage references to objects which 
    ///     have been returned from a query. It's used to check if an object has to be inserted 
    ///     or can be referenced
    /// </summary>
    internal class QueryObjectManager
    {
        private static HashSet<QueryObjectReference> _references = new();

        public static void Initialize()
        {
            TypeBuilder.OnObjectCreated += TypeBuilder_OnObjectCreated;
        }

        public static bool TryGetObjectId(object? obj, out Guid id)
        {
            id = default;
            if (obj == null)
                return false;

            var reference = _references.FirstOrDefault(x => x.Reference.IsAlive && x.Reference.Target == obj);
            id = reference?.ObjectId ?? default;
            return reference != null;
        }

        private static void TypeBuilder_OnObjectCreated(object obj, Guid id)
        {
            var reference = new QueryObjectReference(id, new WeakReference(obj));
            _references.Add(reference);
        }

        private class QueryObjectReference
        {
            public readonly Guid ObjectId;
            public readonly WeakReference Reference;

            public QueryObjectReference(Guid objectId, WeakReference reference)
            {
                ObjectId = objectId;
                Reference = reference;
            }
        }
    }
}
