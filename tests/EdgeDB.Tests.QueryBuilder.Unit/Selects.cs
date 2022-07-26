using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    [TestClass]
    public class Selects : TestBase
    {
        [TestMethod]
        public void SelectScalarAutogenShape()
        {
            var result = QueryBuilder.Select<ScalarType>();
            AssertResultIs(result.Build(), Consts.SCALAR_AUTOGEN_SHAPE);
        }

        [TestMethod]
        public void SelectLinkAutogenShape()
        {
            var result = QueryBuilder.Select<LinkType>();
            AssertResultIs(result.Build(), Consts.SINGLE_LINK_AUTOGEN_SHAPE);
        }
    }
}
