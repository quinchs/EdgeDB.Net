using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryUtils
    {
        internal static string ParseObject(object? obj)
        {
            if (obj is null)
                return "{}";

            if (obj is Enum enm)
            {
                var type = enm.GetType();
                var att = type.GetCustomAttribute<EnumSerializerAttribute>();
                return att != null ? att.Method switch
                {
                    SerializationMethod.Lower => $"\"{obj.ToString()?.ToLower()}\"",
                    SerializationMethod.Numeric => Convert.ChangeType(obj, type.BaseType ?? typeof(int)).ToString() ?? "{}",
                    _ => "{}"
                } : Convert.ChangeType(obj, type.BaseType ?? typeof(int)).ToString() ?? "{}";
            }

            return obj switch
            {
                SubQuery query => query.Query,
                string str => $"\"{str}\"",
                char chr => $"\"{chr}\"",
                Type type => PacketSerializer.GetEdgeQLType(type) ?? type.GetEdgeDBTypeName(),
                _ => obj.ToString()!
            };
        }
    }
}
