using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    public readonly struct Label
    {
        public readonly int Offset;

        public Label(int offset)
        {
            Offset = offset;
        }
    }
}
