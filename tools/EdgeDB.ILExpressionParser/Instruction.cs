using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    public struct Instruction
    {
        public readonly OpCode OpCode;
        public readonly object? Oprand;

        public Instruction(OpCode code, object? oprand)
        {
            OpCode = code;
            Oprand = oprand;
        }
    }
}
