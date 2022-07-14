﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    /// <summary>
    ///     Represents an abstracted form of <see cref="JsonVariable{T}"/>.
    /// </summary>
    internal interface IJsonVariable
    {
        /// <summary>
        ///     Gets the depth of the json.
        /// </summary>
        int Depth { get; }

        /// <summary>
        ///     Gets the name used to reference this json value.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the inner type of the json value.
        /// </summary>
        Type InnerType { get; }

        /// <summary>
        ///     Gets a collection of <see cref="JObject"/> at a specific depth.
        /// </summary>
        /// <param name="targetDepth">The target depth to get the objects at.</param>
        /// <returns>
        ///     A collection of <see cref="JObject"/> at the <paramref name="targetDepth"/>.
        /// </returns>
        IEnumerable<JObject> GetObjectsAtDepth(int targetDepth);
    }

    /// <summary>
    ///     Represents a json value used within queries.
    /// </summary>
    /// <typeparam name="T">The inner type that the json value represents.</typeparam>
    public class JsonVariable<T> : IJsonVariable
    {
        /// <summary>
        ///     Gets the name of the json variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets a mock reference of the json variable.
        /// </summary>
        /// <remarks>
        ///     This property can only be accessed within query builder lambda 
        ///     functions. Attempting to access this property outside of a query 
        ///     builder context will result in a <see cref="InvalidOperationException"/> 
        ///     being thrown.
        /// </remarks>
        public T Value
            => throw new InvalidOperationException("Value cannot be accessed outside of an expression.");

        /// <summary>
        ///     Gets whether or not the inner array is an object array.
        /// </summary>
        internal bool IsObjectArray
            => _array.All(x => x is JObject);

        /// <summary>
        ///     The root <see cref="JArray"/> containing all the json objects.
        /// </summary>
        private readonly JArray _array;

        /// <summary>
        ///     Constructs a new <see cref="JsonVariable{T}"/>.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="array">The <see cref="JArray"/> containing all the json objects.</param>
        internal JsonVariable(string name, JArray array)
        {
            _array = array;
            Name = name;
        }

        /// <inheritdoc cref="IJsonVariable.GetObjectsAtDepth(int)"/>.
        private IEnumerable<JObject> GetObjectsAtDepth(int targetDepth)
        {
            IEnumerable<JObject> GetObjects(JObject obj, int currentDepth)
            {
                if (targetDepth == currentDepth)
                    return new JObject[] { obj };

                if (targetDepth > currentDepth)
                    return obj.Properties().Where(x => x.Value is JObject).SelectMany(x => GetObjects((JObject)x.Value, currentDepth + 1));

                return Array.Empty<JObject>();
            }

            return _array.Where(x => x is JObject).SelectMany(x => GetObjects((JObject)x, 0));
        }

        /// <inheritdoc cref="IJsonVariable.Depth"/>
        private int CalculateDepth()
        {
            return _array.Max(x =>
            {
                if (x is JObject obj)
                    return CalculateNodeDepth(obj, 0);
                return 0;
            });
        }

        /// <summary>
        ///     Calculates the depth of a given json node.
        /// </summary>
        /// <param name="node">The node to calculate depth for.</param>
        /// <param name="depth">The current depth of the computation.</param>
        /// <returns>
        ///     The depth of the given <paramref name="node"/>.
        /// </returns>
        private int CalculateNodeDepth(JObject node, int depth = 0)
        {
            return node.Properties().Max(x =>
            {
                if (x.Value is not JObject jObject)
                    return depth;

                return CalculateNodeDepth(jObject, depth + 1);
            });
        }

        string IJsonVariable.Name => Name;
        Type IJsonVariable.InnerType => typeof(T);
        IEnumerable<JObject> IJsonVariable.GetObjectsAtDepth(int targetDepth) => GetObjectsAtDepth(targetDepth);
        int IJsonVariable.Depth => CalculateDepth();
    }
}
