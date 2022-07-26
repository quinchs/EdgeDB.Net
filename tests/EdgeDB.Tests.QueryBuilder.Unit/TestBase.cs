using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Tests.Unit
{
    public abstract class TestBase
    {
        protected void AssertResultIs(BuiltQuery result, string matching)
        {
            // TODO: variable inference with matching since variables are randomly generated.
            var queryText = result.Query;
            Assert.Equals(queryText, matching);
        }
    }
}
