using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    internal class QueryGlobal
    {
        public object? Value { get; init; }
        public object? Reference { get; init; }
        public string Name { get; init; }

        public QueryGlobal(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public QueryGlobal(string name, object? value, object? reference)
        {
            Name = name;
            Value = value;
            Reference = reference;
        }
    }
}
