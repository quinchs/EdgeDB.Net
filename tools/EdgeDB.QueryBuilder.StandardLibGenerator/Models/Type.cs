using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.StandardLibGenerator.Models
{
    [EdgeDBType(ModuleName = "schema")]
    internal class Type
    {
        public string Name { get; set; }
        public bool IsAbstract { get; set; }

        [EdgeDBIgnore]
        public string TypeOfSelf { get; set; }

        [EdgeDBIgnore]
        public Guid Id { get; set; }

        [EdgeDBDeserializer]
        public Type(IDictionary<string, object?> raw)
        {
            Name = (string)raw["name"]!;
            IsAbstract = (bool)raw["is_abstract"]!;
            TypeOfSelf = (string)raw["__tname__"]!;
            Id = (Guid)raw["id"]!;
        }

        public async Task<MetaType> GetMetaInfoAsync(EdgeDBClient client)
        {
            var result = await QueryBuilder.Select<MetaType>((ctx) => new MetaType
            {
                Pointers = ctx.Raw<Pointer[]>("[is schema::ObjectType].pointers { name, type: {name, is_abstract}}"),
            }).Filter(x => x.Id == Id).ExecuteAsync(client);

            return result.First()!;
        }
    }

    [EdgeDBType("Type", ModuleName = "schema")]
    internal class MetaType
    {
        public Guid Id { get; set; }
        public Pointer[]? Pointers { get; set; }
    }

    internal class Pointer
    {
        public string? Name { get; set; }
        public Type? Type { get; set; }
    }
}
