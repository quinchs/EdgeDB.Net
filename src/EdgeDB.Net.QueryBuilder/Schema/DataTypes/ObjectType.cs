using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema.DataTypes
{
    [EdgeDBType(ModuleName = "schema")]
    internal class ObjectType
    {
        [EdgeDBIgnore]
        public string CleanedName
            => Name!.Split("::")[1];
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool IsAbstract { get; set; }
        [EdgeDBProperty("pointers")]
        public Property[]? Properties { get; set; }
    }
}
