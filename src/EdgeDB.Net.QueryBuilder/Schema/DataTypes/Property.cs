using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Schema.DataTypes
{
    public enum Cardinality
    {
        One,
        AtMostOne,
        AtLeastOne,
        Many,
    }
    internal class Property
    {
        [EdgeDBProperty("real_cardinality")]
        public Cardinality Cardinality { get; set; }
        public string? Name { get; set; }
        public Guid? TargetId { get; set; }
        public bool IsLink { get; set; }
        public bool IsExclusive { get; set; }
        public bool IsComputed { get; set; }
        public bool IsReadonly { get; set; }
        public bool HasDefault { get; set; }
    }
}
