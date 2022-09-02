﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class ReflectionUtils
    {
        public static object? GetValueTypeDefault(Type type)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var valueProperty = type.GetProperty("Value")!;
                type = valueProperty.PropertyType;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static bool IsSubTypeOfGenericType(Type genericType, Type toCheck)
        {
            Type? type = toCheck;
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
                type = type.BaseType;
            }
            return false;
        }

        public static bool TryGetRawGeneric(Type generic, Type? toCheck, out Type? genericReference)
        {
            genericReference = null;

            while (toCheck is not null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    genericReference = toCheck;

                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static object? DynamicCast(object? entity, Type to)
            => typeof(ReflectionUtils).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(to).Invoke(null, new object?[] { entity });

        private static T? Cast<T>(object? entity)
            => (T?)entity;

        public static object? GetDefault(Type t)
            => typeof(ReflectionUtils).GetMethod("GetDefaultGeneric", BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(t).Invoke(null, null);

        private static object? GetDefaultGeneric<T>() => default(T);
    }
}
