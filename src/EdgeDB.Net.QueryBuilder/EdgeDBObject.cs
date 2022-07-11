using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public class EdgeDBObject
    {
        [EdgeDBProperty("id")]
        public Guid Id { get; }

        [EdgeDBDeserializer]
        internal EdgeDBObject(IDictionary<string, object?> data)
        {
            Id = (Guid)data["id"]!;
        }
    }
}
