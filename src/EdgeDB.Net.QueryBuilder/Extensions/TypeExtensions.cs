using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal static class TypeExtensions
    {
        public static string GetEdgeDBTypeName(this Type type)
            => type.GetCustomAttribute<EdgeDBTypeAttribute>()?.Name ?? type.Name;
    }
}
