using EdgeDB.Schema.DataTypes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema
{
    internal class SchemaInfo
    {
        public IReadOnlyCollection<ObjectType> Types { get; }
        public SchemaInfo(IReadOnlyCollection<ObjectType?> types)
        {
            Types = types!;
        }

        public bool TryGetObjectInfo(Type type, [MaybeNullWhen(false)] out ObjectType info)
            => (info = Types.FirstOrDefault(x => x.CleanedName == type.GetEdgeDBTypeName())) != null;
    }
}
