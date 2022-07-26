using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    public class Consts
    {
        public const string SCALAR_AUTOGEN_SHAPE = "select ScalarType { string_prop, long_prop, date_time_offset_prop }";
        public const string SINGLE_LINK_AUTOGEN_SHAPE = "select LinkType { string_prop, link_prop: { string_prop } }";
    }
}
