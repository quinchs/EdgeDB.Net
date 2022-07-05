﻿using EdgeDB.Serializer;
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
        {
            var attr = type.GetCustomAttribute<EdgeDBTypeAttribute>();
            var name = attr?.Name ?? type.Name;
            return attr != null ? $"{(attr.ModuleName != null ? $"{attr.ModuleName}::" : "")}{name}" : name;
        }
        public static string GetEdgeDBPropertyName(this MemberInfo info)
            => info.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? TypeBuilder.NamingStrategy.GetName(info);

        public static Type GetMemberType(this MemberInfo info)
        {
            switch (info)
            {
                case PropertyInfo propertyInfo:
                    return propertyInfo.PropertyType;
                case FieldInfo fieldInfo:
                    return fieldInfo.FieldType;
                default:
                    throw new NotSupportedException();
            }
        }

        public static object? GetMemberValue(this MemberInfo info, object? obj)
        {
            return info switch
            {
                FieldInfo field => field.GetValue(obj),
                PropertyInfo property => property.GetValue(obj),
                _ => throw new InvalidOperationException("Cannot resolve constant member expression")
            };
        }
    }
}
