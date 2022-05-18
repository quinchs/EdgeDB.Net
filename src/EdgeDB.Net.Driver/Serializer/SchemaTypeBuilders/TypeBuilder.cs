﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public static class TypeBuilder
    {
        public static IReadOnlyDictionary<Type, SchemaTypeInfo> CustomTypeBuilders
            => _customTypeBuilders.ToImmutableDictionary();

        internal static ConcurrentDictionary<Type, SchemaTypeInfo> _customTypeBuilders = new();
        internal static ConcurrentDictionary<Type, SchemaTypeInfo> _typeInfo = new();

        public static SchemaTypeInfo AddOrUpdateCustomTypeBuilder<TType>(Action<TType, IDictionary<string, object?>> builder)
        {
            var info = new SchemaTypeInfo(typeof(TType));
            info.CustomBuilder = (inst, data) => builder((TType)inst, data);
            return _customTypeBuilders.AddOrUpdate(typeof(TType), info, (_, __) => info);
        }

        public static bool TryRemoveCustomTypeBuilder<TType>([MaybeNullWhen(false)] out SchemaTypeInfo info)
            => _customTypeBuilders.TryRemove(typeof(TType), out info); 

        internal static object? BuildObject(Type type, IDictionary<string, object?> raw)
        {
            if (!IsValidObjectType(type))
                throw new InvalidOperationException($"Cannot use {type.Name} to deserialize to");

            var info = _typeInfo.GetOrAdd(type, (_) => new SchemaTypeInfo(type));

            return info.Create(raw);
        }

        internal static bool IsValidObjectType(Type type)
        {
            // check constructor for builder
            var validConstructor = type.GetConstructor(Array.Empty<Type>()) != null ||
                                   type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;

            return (type.IsClass || type.IsValueType) && !type.IsSealed && validConstructor;
        }
    }

    public class SchemaTypeInfo
    {
        public Type ObjectType { get; set; }
        public Action<object, IDictionary<string, object?>>? CustomBuilder { get; set; }
        public bool ConstructorIsBuilder { get; private set; }

        private Dictionary<string, PropertyInfo> _propertyMap;
        internal SchemaTypeInfo(Type type)
        {
            ObjectType = type;

            _propertyMap = GetPropertyMap();

            if(TryGetCustomBuilder(out var methodInfo))
            {
                CustomBuilder = (inst, data) => methodInfo!.Invoke(inst, new object[] { data });
            }

            ConstructorIsBuilder = type.GetConstructor(new Type[] { typeof(IDictionary<string, object?>) })?.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null;
        }

        internal object? Create(IDictionary<string, object?> rawValue)
        {
            if (ConstructorIsBuilder)
                return Activator.CreateInstance(ObjectType, rawValue);

            var instance = Activator.CreateInstance(ObjectType);

            if (instance is null)
                throw new TargetInvocationException($"Cannot create an instance of {ObjectType.Name}", null);

            if (CustomBuilder is not null)
            {
                CustomBuilder(instance, rawValue);
                return instance;
            }

            foreach(var prop in _propertyMap)
            {
                if (rawValue.TryGetValue(prop.Key, out var value))
                    prop.Value.SetValue(instance, value);
            }

            return instance;
        }

        private bool TryGetCustomBuilder(out MethodInfo? info)
        {
            info = null;
            var method = ObjectType.GetMethods().FirstOrDefault(x =>
            {
                if (x.GetCustomAttribute<EdgeDBDeserializerAttribute>() != null && x.ReturnType == typeof(void))
                {
                    var parameters = x.GetParameters();

                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(IDictionary<string, object?>);
                }

                return false;
            });

            info = method;
            return method is not null;
        }

        private Dictionary<string, PropertyInfo> GetPropertyMap()
        {
            var properties = ObjectType.GetProperties().Where(x =>
                x.CanWrite &&
                x.GetCustomAttribute<EdgeDBIgnoreAttribute>() == null &&
                x.SetMethod != null);

            return properties.ToDictionary(x => x.GetCustomAttribute<EdgeDBPropertyAttribute>()?.Name ?? x.Name,x => x);
        }
    }
}