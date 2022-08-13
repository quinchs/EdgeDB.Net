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
    public unsafe struct Instruction
    {
        public readonly OpCode OpCode;
        public readonly object? Oprand;

        private readonly MethodBase _rootMethod;

        private readonly void* _op;

        public Instruction(OpCode code, object? oprand, MethodBase method)
        {
            _op = Unsafe.AsPointer(ref oprand);
            OpCode = code;
            Oprand = oprand;
            _rootMethod = method;
        }

        public T OprandAs<T>()
            => Unsafe.Read<T>(_op);

        public MethodBase OprandAsMethod()
        {
            if (OpCode.OperandType != OperandType.InlineMethod && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a method");

            return _rootMethod.Module.ResolveMethod(OprandAs<int>(), _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public Type OprandAsType()
        {
            if (OpCode.OperandType != OperandType.InlineType && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a type");

            return _rootMethod.Module.ResolveType(OprandAs<int>(), _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public FieldInfo OprandAsField()
        {
            if (OpCode.OperandType != OperandType.InlineField && OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("The current instruction doesn't reference a field");

            return _rootMethod.Module.ResolveField(OprandAs<int>(), _rootMethod.DeclaringType!.GenericTypeArguments, _rootMethod.GetGenericArguments())!;
        }

        public string OprandAsString()
        {
            if (OpCode.OperandType != OperandType.InlineString)
                throw new InvalidOperationException("The current instruction doesn't reference a string");

            return _rootMethod.Module.ResolveString(OprandAs<int>());
        }
        
        public MemberInfo OprandAsMember()
        {
            if (OpCode.OperandType != OperandType.InlineTok)
                throw new InvalidOperationException("Instruction does not reference a member.");

            return _rootMethod.Module.ResolveMember(OprandAs<int>())!;
        }

        public byte[] OprandAsSignature()
        {
            if (OpCode.OperandType != OperandType.InlineSig)
                throw new InvalidOperationException("Instruction does not reference a signature.");
            return _rootMethod.Module.ResolveSignature(OprandAs<int>());
        }

        public object? ParseOprand()
        {
            return OpCode.OperandType switch
            {
                OperandType.InlineBrTarget => OprandAs<Label>(),
                OperandType.InlineField => OprandAsField(),
                OperandType.InlineI => OprandAs<int>(),
                OperandType.InlineI8 => OprandAs<long>(),
                OperandType.InlineMethod => OprandAsMethod(),
                OperandType.InlineNone => null,
                OperandType.InlineR => OprandAs<double>(),
                OperandType.InlineSig => OprandAsSignature(),
                OperandType.InlineString => OprandAsString(),
                OperandType.InlineSwitch => OprandAs<int>(),
                OperandType.InlineTok => OprandAsMember(),
                OperandType.InlineType => OprandAsType(),
                OperandType.InlineVar => OprandAs<short>(),
                OperandType.ShortInlineBrTarget => OprandAs<Label>(),
                OperandType.ShortInlineI => OprandAs<byte>(),
                OperandType.ShortInlineR => OprandAs<float>(),
                OperandType.ShortInlineVar => OprandAs<byte>(),

                _ => Oprand
            };
        }
    }
}
