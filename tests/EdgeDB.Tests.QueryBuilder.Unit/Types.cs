using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    public class ScalarType
    {
        public string? StringProp { get; set; }
        public long LongProp { get; set; }
        public DateTimeOffset DateTimeOffsetProp { get; set; }
    }

    public class LinkType
    {
        public string? StringProp { get; set; }
        public LinkType? LinkProp { get; set; }
    }

    public class MultiLinkType
    {
        public string? StringProp { get; set; }
        public LinkType? LinkProp { get; set; }
        public LinkType[]? MultiLinkProp { get; set; }
    }

    public class InheritanceType : ScalarType
    {
        public LinkType? LinkProp { get; set; }
    }
}
