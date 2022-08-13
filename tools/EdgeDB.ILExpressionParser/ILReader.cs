using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    public unsafe ref struct ILReader
    {
        public readonly MethodBody MethodBody;
        public readonly MethodBase MethodBase;
        private readonly Span<byte> _il;
        private int _position;
        private byte CurrentByte
            => _il[_position];

        private byte NextByte
            => _il[_position + 1];

        public Label MarkPosition() => new Label(_position);

        public ILReader(MethodBase method)
        {
            MethodBase = method;
            var bodyInfo = method.GetMethodBody()!;
            _position = 0;
            MethodBody = bodyInfo;
            _il = bodyInfo.GetILAsByteArray();
        }

        public void Seek(Label label)
            => _position = label.Offset;

        public Span<byte> PeekBytes(int count)
            => _il[..count];

        public Span<byte> PeekBytes(int index, int count)
            => _il[index..count];

        public bool ReadNext(out Instruction instruction)
        {
            instruction = default;
            if (_position >= _il.Length)
                return false;

            var opCode = GetOpCode();
            object? oprand;
            
            switch (opCode.OperandType)
            {
                case OperandType.InlineSwitch:
                    oprand = new Label(_position);
                    var length = BitConverter.ToInt32(_il[_position..sizeof(int)]);
                    _position += 4 + 4 * length;
                    break;
                case OperandType.InlineI:
                case OperandType.InlineMethod:
                case OperandType.InlineString:
                case OperandType.InlineSig:
                case OperandType.InlineField:
                case OperandType.InlineType:
                case OperandType.InlineTok:
                    oprand = BitConverter.ToInt32(_il[_position..sizeof(int)]);
                    _position += sizeof(int);
                    break;
                case OperandType.InlineI8:
                    oprand = BitConverter.ToInt64(_il[_position..sizeof(long)]);
                    _position += sizeof(long);
                    break;
                case OperandType.InlineR:
                    oprand = BitConverter.ToDouble(_il[_position..sizeof(double)]);
                    _position += sizeof(double);
                    break;
                case OperandType.InlineVar:
                    oprand = BitConverter.ToInt16(_il[_position..sizeof(short)]);
                    _position += sizeof(short);
                    break;
                case OperandType.ShortInlineR:
                    oprand = BitConverter.ToSingle(_il[_position..sizeof(float)]);
                    _position += sizeof(float);
                    break;
                case OperandType.InlineBrTarget:
                    oprand = new Label(BitConverter.ToInt32(_il[_position..sizeof(int)]) + _position + 4);
                    _position += sizeof(int);
                    break;
                case OperandType.ShortInlineBrTarget:
                    oprand = new Label(_il[_position] + _position + 1);
                    _position += 1;
                    break;
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    oprand = _il[_position];
                    _position += 1;
                    break;
                case OperandType.InlineNone:
                    oprand = null;
                    break;
                default:
                    throw new NotImplementedException($"No implementation found for oprand {opCode}");
            }

            instruction = new(opCode, oprand, MethodBase);
            return true;
        }

        private OpCode GetOpCode()
        {
            OpCodes code = CurrentByte == 0xfe 
                ? (OpCodes)((CurrentByte << 2) + NextByte) 
                : (OpCodes)CurrentByte;

            _position += CurrentByte == 0xfe ? 2 : 1;

            if (!Utils.TryGetOpCode(code, out var opCode))
                throw new Exception($"Unknown opcode {code}");

            return opCode;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
