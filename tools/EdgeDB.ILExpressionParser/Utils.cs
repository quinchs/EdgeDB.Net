using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    internal class Utils
    {
        private static Dictionary<OpCodes, OpCode> _opCodes;

        static Utils()
        {
            _opCodes = new Dictionary<OpCodes, OpCode>(typeof(System.Reflection.Emit.OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(x =>
                {
                    var val = (OpCode)x.GetValue(null)!;
                    return new KeyValuePair<OpCodes, OpCode>((OpCodes)val.Value, val);
                }));
        }

        public static bool TryGetOpCode(OpCodes code, out OpCode opCode)
            => _opCodes.TryGetValue(code, out opCode);
    }
}
