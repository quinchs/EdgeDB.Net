using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    public class ExpressionParser
    {
        public static Expression<T> Parse<T>(Delegate func)
        {
            var reader = new ILReader(func.Method);
            var locals = reader.MethodBody.LocalVariables.Select(x => Expression.Variable(x.LocalType, $"local_{x.LocalIndex}")).ToArray();
            var builderArgs = func.Method.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name));
            var funcArgs = builderArgs.ToArray();
            if (!func.Method.IsStatic)
                builderArgs = builderArgs.Prepend(Expression.Parameter(func.Method.DeclaringType!, "this"));
            return Expression.Lambda<T>(BuildExpression(ref reader, builderArgs.ToArray(), locals), funcArgs); // TODO: parameters
        }

        private static Expression BuildExpression(ref ILReader reader, ParameterExpression[] arguments, ParameterExpression[] locals)
        {
            var expressionStack = new Stack<Expression>();
            var referenceStack = new Stack<object?>();
            var branchs = new Dictionary<Label, Expression>();
            bool isTailCall = false;
            
            while (reader.ReadNext(out var instruction))
            {
                switch ((OpCodes)instruction.OpCode.Value)
                {
                    #region Numerical
                    case OpCodes.Add_ovf or OpCodes.Add_ovf_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.AddChecked(left, right));
                        }
                        break;
                    case OpCodes.Sub_ovf or OpCodes.Sub_ovf_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.SubtractChecked(left, right));
                        }
                        break;
                    case OpCodes.Mul_ovf or OpCodes.Mul_ovf_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.MultiplyChecked(left, right));
                        }
                        break;
                    case OpCodes.Div or OpCodes.Div_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Divide(left, right));
                        }
                        break;
                    case OpCodes.Add:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Add(left, right));
                        }
                        break;
                    case OpCodes.Sub:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Subtract(left, right));
                        }
                        break;
                    case OpCodes.Mul:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Multiply(left, right));
                        }
                        break;
                    case OpCodes.Neg:
                        {
                            var value = expressionStack.Pop();
                            expressionStack.Push(Expression.Negate(value));
                        }
                        break;
                    case OpCodes.Rem or OpCodes.Rem_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Modulo(left, right));
                        }
                        break;
                    #endregion
                    #region Bitwise
                    case OpCodes.And:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.And(left, right));
                        }
                        break;
                    case OpCodes.Or:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Or(left, right));
                        }
                        break;
                    case OpCodes.Xor:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.ExclusiveOr(left, right));
                        }
                        break;
                    case OpCodes.Not:
                        {
                            var value = expressionStack.Pop();
                            expressionStack.Push(Expression.Not(value));
                        }
                        break;
                    case OpCodes.Shl:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.LeftShift(left, right));
                        }
                        break;
                    case OpCodes.Shr or OpCodes.Shr_un:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.RightShift(left, right));
                        }
                        break;
                    case OpCodes.Box:
                        {
                            expressionStack.Push(expressionStack.Pop());
                        }
                        break;
                    #endregion
                    #region Branching
                    case OpCodes.Br or OpCodes.Br_s:
                        {
                            throw new Exception("TODO: BR(_S)");
                        }
                        break;
                    case OpCodes.Brtrue:
                    case OpCodes.Brtrue_s:
                        {
                            var condition = expressionStack.Pop();
                            var position = reader.MarkPosition();
                            var trueLabel = (Label)instruction.Oprand!;

                            if (!reader.ReadNext(out var target))
                                throw new Exception("No instruction after a branch");

                            if (!branchs.TryGetValue(position, out var elseExpression))
                                elseExpression = branchs[position] = BuildExpression(ref reader, arguments, locals);
                            if(!branchs.TryGetValue(trueLabel, out var trueExpression))
                            {
                                reader.Seek(trueLabel);
                                trueExpression = branchs[trueLabel] = BuildExpression(ref reader, arguments, locals);
                            }

                            expressionStack.Push(Expression.Condition(condition, trueExpression, elseExpression));
                        }
                        break;
                    case OpCodes.Switch:
                        {
                            var value = expressionStack.Pop();
                            if (!reader.ReadNext(out _))
                                throw new Exception("no switch body");

                            var switchLocation = reader.MarkPosition();

                            var branches = GetBranches((int)instruction.Oprand!, ref reader);
                            SwitchCase[] cases = new SwitchCase[branches.Count];
                            for(int i = 0; i != branches.Count; i++)
                            {
                                var kvp = branches.ElementAt(i);
                                if(!branchs.TryGetValue(kvp.Value, out var caseExpression))
                                {
                                    reader.Seek(kvp.Value);
                                    caseExpression = BuildExpression(ref reader, arguments, locals);
                                }
                                cases[i] = Expression.SwitchCase(caseExpression, Expression.Constant(kvp.Key));
                            }

                            Expression.Switch(value, cases);
                            reader.Seek(switchLocation);
                        }
                        break;
                    case OpCodes.Leave or OpCodes.Leave_s:
                        {
                            throw new Exception("TODO: leave labels");
                            //reader.Seek(instruction.OprandAs<Label>()!);
                            //expressionStack.Push(Expression.Goto(Expression.Label()));
                        }
                        break;
                    case OpCodes.Endfilter:
                        {
                            throw new Exception("TODO: endfilter");
                        }
                        break;
                    case OpCodes.Endfinally:
                        {
                            throw new Exception("TODO: endfinally");
                        }
                        break;
                    #endregion
                    #region Method calls
                    case OpCodes.Ldftn:
                        referenceStack.Push(instruction.OprandAsMethod());
                        break;
                    case OpCodes.Ldvirtftn:
                        throw new Exception("TODO: ldvirtfnt");
                    case OpCodes.Calli:
                        throw new Exception("TODO: Calli");
                    case OpCodes.Call or OpCodes.Callvirt:
                        {
                            var method = instruction.OprandAsMethod();
                            var args = method.GetParameters().Select((p, i) =>
                            {
                                var arg = expressionStack.Pop();
                                if (arg.Type != p.ParameterType)
                                    return Expression.TypeAs(arg, p.ParameterType);
                                return arg;
                            }).Reverse().ToArray();

                            var inst = method.IsStatic ? null : expressionStack.Pop();

                            if(method.GetCustomAttribute<CompilerGeneratedAttribute>() != null && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                            {
                                // property get/set
                                var propName = method.Name[..4];
                                var prop = method.DeclaringType!.GetProperty(propName)!;
                                expressionStack.Push(args.Any() ? Expression.Property(inst, prop, args) : Expression.Property(inst, prop));
                            }
                            else
                                expressionStack.Push(Expression.Call(inst, (MethodInfo)method, args));
                        }
                        break;
                    case OpCodes.Jmp:
                        throw new Exception("TODO: jmp");
                    case OpCodes.Cpblk:
                        throw new Exception("TODO: cpblk");
                    case OpCodes.Cpobj:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(left, right));
                        }
                        break;
                    case OpCodes.Initblk:
                        throw new Exception("TODO: initblk");
                    case OpCodes.Initobj:
                        throw new Exception("TODO: initobj");
                    #endregion
                    #region Load/Store
                    case OpCodes.Sizeof:
                        {
                            var type = instruction.OprandAsType();
                            expressionStack.Push(Expression.Constant(Marshal.SizeOf(type)));
                        }
                        break;
                    case OpCodes.Localloc:
                        throw new Exception("TODO: localloc");
                    case OpCodes.Ldsflda or OpCodes.Ldflda:
                        throw new Exception("TODO: ldsflda & ldflda");
                    case OpCodes.Ldloca or OpCodes.Ldloca_s:
                        throw new Exception("TODO: ldloca & ldloca_s");
                    case OpCodes.Ldarga or OpCodes.Ldarga_s:
                        throw new Exception("TODO: ldarga & ldarga_s");
                    case OpCodes.Ldarg_0:
                        expressionStack.Push(arguments[0]);
                        break;
                    case OpCodes.Ldarg_1:
                        expressionStack.Push(arguments[1]);
                        break;
                    case OpCodes.Ldarg_2:
                        expressionStack.Push(arguments[2]);
                        break;
                    case OpCodes.Ldarg_3:
                        expressionStack.Push(arguments[3]);
                        break;
                    case OpCodes.Ldc_i4_m1:
                        expressionStack.Push(Expression.Constant(-1));
                        break;
                    case OpCodes.Ldc_i4_0:
                        expressionStack.Push(Expression.Constant(0));
                        break;
                    case OpCodes.Ldc_i4_1:
                        expressionStack.Push(Expression.Constant(1));
                        break;
                    case OpCodes.Ldc_i4_2:
                        expressionStack.Push(Expression.Constant(2));
                        break;
                    case OpCodes.Ldc_i4_3:
                        expressionStack.Push(Expression.Constant(3));
                        break;
                    case OpCodes.Ldc_i4_4:
                        expressionStack.Push(Expression.Constant(4));
                        break;
                    case OpCodes.Ldc_i4_5:
                        expressionStack.Push(Expression.Constant(5));
                        break;
                    case OpCodes.Ldc_i4_6:
                        expressionStack.Push(Expression.Constant(6));
                        break;
                    case OpCodes.Ldc_i4_7:
                        expressionStack.Push(Expression.Constant(7));
                        break;
                    case OpCodes.Ldc_i4_8:
                        expressionStack.Push(Expression.Constant(8));
                        break;
                    case OpCodes.Ldc_i4 or OpCodes.Ldc_i4_s:
                        {
                            var value = instruction.OprandAs<int>();
                            expressionStack.Push(Expression.Constant(value));
                        }
                        break;
                    case OpCodes.Ldc_i8:
                        {
                            var value = instruction.OprandAs<long>();
                            expressionStack.Push(Expression.Constant(value));
                        }
                        break;
                    case OpCodes.Ldc_r8:
                        {
                            var value = instruction.OprandAs<double>();
                            expressionStack.Push(Expression.Constant(value));
                        }
                        break;
                    case OpCodes.Ldc_r4:
                        {
                            var value = instruction.OprandAs<float>();
                            expressionStack.Push(Expression.Constant(value));
                        }
                        break;
                    case OpCodes.Ldfld or OpCodes.Ldsfld:
                        {
                            var field = instruction.OprandAsField();
                            var instance = field.IsStatic ? null : expressionStack.Pop();
                            expressionStack.Push(Expression.Field(instance, field));
                        }
                        break;
                    case OpCodes.Ldlen:
                        {
                            var arr = expressionStack.Pop();
                            expressionStack.Push(Expression.ArrayLength(arr));
                        }
                        break;
                    case OpCodes.Ldloc_0:
                        expressionStack.Push(locals[0]);

                        break;
                    case OpCodes.Ldloc_1:
                        expressionStack.Push(locals[1]);

                        break;
                    case OpCodes.Ldloc_2:
                        expressionStack.Push(locals[2]);

                        break;
                    case OpCodes.Ldloc_3:
                        expressionStack.Push(locals[3]);

                        break;
                    case OpCodes.Ldloc or OpCodes.Ldloc_s:
                        {
                            var local = reader.MethodBody.LocalVariables[instruction.OprandAs<int>()];
                            expressionStack.Push(Expression.Variable(local.LocalType, $"local_{local.LocalIndex}"));
                        }
                        break;
                    case OpCodes.Stloc_0:
                        {
                            var val = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(locals[0], val));
                        }
                        break;
                    case OpCodes.Stloc_1:
                        {
                            var val = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(locals[1], val));
                        }
                        break;
                    case OpCodes.Stloc_2:
                        {
                            var val = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(locals[2], val));
                        }
                        break;
                    case OpCodes.Stloc_3:
                        {
                            var val = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(locals[3], val));
                        }
                        break;
                    case OpCodes.Stloc:
                        {
                            var val = expressionStack.Pop();
                            var indx = instruction.OprandAs<short>();
                            expressionStack.Push(Expression.Assign(locals[indx], val));
                        }
                        break;
                    case OpCodes.Starg or OpCodes.Starg_s:
                        {
                            var val = expressionStack.Pop();
                            var indx = instruction.OprandAs<short>();
                            expressionStack.Push(Expression.Assign(arguments[indx], val));
                        }
                        break;
                    case OpCodes.Stobj:
                        {
                            var val = expressionStack.Pop();
                            var target = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(target, val));
                        }
                        break;
                    case OpCodes.Stsfld or OpCodes.Stfld:
                        {
                            var field = instruction.OprandAsField();
                            var val = expressionStack.Pop();
                            var inst = field.IsStatic ? null : expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(Expression.Field(inst, field), val));
                        }
                        break;
                    case OpCodes.Ldnull:
                        expressionStack.Push(Expression.Constant(null));
                        break;
                    case OpCodes.Ldstr:
                        expressionStack.Push(Expression.Constant(instruction.OprandAsString()));
                        break;
                    case OpCodes.Ldtoken:
                        expressionStack.Push(Expression.Constant(instruction.ParseOprand()));
                        break;
                    case OpCodes.Nop:
                        break;
                    case OpCodes.Pop:
                        expressionStack.Pop();
                        break;
                    case OpCodes.Ret:
                        break;
                        //throw new Exception("TODO: ret");
                    case OpCodes.Ldelem
                        or OpCodes.Ldelem_ref
                        or OpCodes.Ldelem_i
                        or OpCodes.Ldelem_i1
                        or OpCodes.Ldelem_i2
                        or OpCodes.Ldelem_i4
                        or OpCodes.Ldelem_i8
                        or OpCodes.Ldelem_u1
                        or OpCodes.Ldelem_u2
                        or OpCodes.Ldelem_u4
                        or OpCodes.Ldelem_r4
                        or OpCodes.Ldelem_r8:
                        {
                            var indx = expressionStack.Pop();
                            var arr = expressionStack.Pop();
                            expressionStack.Push(Expression.ArrayIndex(arr, indx));
                        }
                        break;
                    case OpCodes.Stelem
                        or OpCodes.Stelem_ref
                        or OpCodes.Stelem_i
                        or OpCodes.Stelem_i1
                        or OpCodes.Stelem_i2
                        or OpCodes.Stelem_i4
                        or OpCodes.Stelem_i8
                        or OpCodes.Stelem_r4
                        or OpCodes.Stelem_r8:
                        {
                            var val = expressionStack.Pop();
                            var indx = expressionStack.Pop();
                            var arr = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(Expression.ArrayAccess(arr, indx), val));
                        }
                        break;
                    case OpCodes.Ldind_ref
                        or OpCodes.Ldind_i
                        or OpCodes.Ldind_i1
                        or OpCodes.Ldind_i2
                        or OpCodes.Ldind_i4
                        or OpCodes.Ldind_i8
                        or OpCodes.Ldind_r4
                        or OpCodes.Ldind_r8
                        or OpCodes.Ldind_u1
                        or OpCodes.Ldind_u2
                        or OpCodes.Ldind_u4:
                        break; // don't do anything for address getting
                    case OpCodes.Stind_ref
                        or OpCodes.Stind_i
                        or OpCodes.Stind_i1
                        or OpCodes.Stind_i2
                        or OpCodes.Stind_i4
                        or OpCodes.Stind_i8
                        or OpCodes.Stind_r4
                        or OpCodes.Stind_r8:
                        {
                            var val = expressionStack.Pop();
                            var target = expressionStack.Pop();
                            expressionStack.Push(Expression.Assign(target, val));
                        }
                        break;
                    case OpCodes.Arglist:
                        throw new Exception("TODO: arglist");
                    #endregion
                    #region Comparison
                    case OpCodes.Isinst:
                        expressionStack.Push(Expression.TypeIs(expressionStack.Pop(), instruction.OprandAsType()));
                        break;
                    case OpCodes.Ceq:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Equal(left, right));
                        }
                        break;
                    case OpCodes.Beq or OpCodes.Beq_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.Equal(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Bge or OpCodes.Bge_un or OpCodes.Bge_un_s or OpCodes.Bge_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.GreaterThanOrEqual(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Ble_s or OpCodes.Ble or OpCodes.Ble_un or OpCodes.Ble_un_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.LessThanOrEqual(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Blt or OpCodes.Blt_s or OpCodes.Blt_un or OpCodes.Blt_un_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.LessThan(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Bgt or OpCodes.Bgt_s or OpCodes.Bgt_un or OpCodes.Bgt_un_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.GreaterThan(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Bne_un or OpCodes.Bne_un_s:
                        {
                            var right = expressionStack.Pop();
                            var left = expressionStack.Pop();
                            expressionStack.Push(Expression.NotEqual(left, right));
                            goto case OpCodes.Brtrue;
                        }
                        break;
                    case OpCodes.Brfalse or OpCodes.Brfalse_s:
                        {
                            var left = expressionStack.Pop();
                            // TODO: value type defaults?
                            expressionStack.Push(Expression.Equal(left, Expression.Constant(left.Type.IsValueType ? default : null, left.Type)));
                        }
                        break;
                    case OpCodes.Ckfinite:
                        {
                            var arg = expressionStack.Pop();
                            var checker = arg.Type == typeof(double)
                                ? typeof(double).GetMethod("IsInfinity")
                                : typeof(float).GetMethod("IsInfinity");

                            var body = Expression.Throw(
                                Expression.New(
                                    typeof(ArithmeticException).GetConstructor(new[] { typeof(string) })!, 
                                    Expression.Constant("Value is not finite")));

                            expressionStack.Push(Expression.IfThen(Expression.Call(null, checker!, arg), body));
                        }
                        break;
                    case OpCodes.Constrained_:
                        throw new Exception("TODO: constrained.");
                    #endregion
                    #region Conversion
                    case OpCodes.Castclass:
                        {
                            var value = expressionStack.Pop();
                            var targetType = instruction.OprandAsType();
                            if (value.Type == targetType)
                                expressionStack.Push(value);

                            expressionStack.Push(CastTo(value, targetType));
                        }
                        break;
                    case OpCodes.Conv_i:
                        expressionStack.Push(CastTo(expressionStack.Pop(), UIntPtr.Size == 4 ? typeof(int) : typeof(long)));
                        break;
                    case OpCodes.Conv_i1:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(sbyte)));
                        break;
                    case OpCodes.Conv_i2:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(short)));
                        break;
                    case OpCodes.Conv_i4:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(int)));
                        break;
                    case OpCodes.Conv_i8:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(long)));
                        break;
                    case OpCodes.Conv_ovf_i or OpCodes.Conv_ovf_i_un:
                        expressionStack.Push(CastToChecked(expressionStack.Pop(), UIntPtr.Size == 4 ? typeof(int) : typeof(long)));
                        break;
                    case OpCodes.Conv_ovf_i1 or OpCodes.Conv_ovf_i1_un:
                        expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(sbyte)));
                        break;
                    case OpCodes.Conv_ovf_i2 or OpCodes.Conv_ovf_i2_un:
                        expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(short)));
                        break;
                    case OpCodes.Conv_ovf_i4 or OpCodes.Conv_ovf_i4_un:
                        expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(int)));
                        break;
                    case OpCodes.Conv_ovf_i8 or OpCodes.Conv_ovf_i8_un:
                        expressionStack.Push(CastToChecked(expressionStack.Pop(), typeof(long)));
                        break;
                    case OpCodes.Conv_r4 or OpCodes.Conv_r_un:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(float)));
                        break;
                    case OpCodes.Conv_r8:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(double)));
                        break;
                    case OpCodes.Conv_u:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(nuint)));
                        break;
                    case OpCodes.Conv_u1:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(byte)));
                        break;
                    case OpCodes.Conv_u2:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(ushort)));
                        break;
                    case OpCodes.Conv_u4:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(uint)));
                        break;
                    case OpCodes.Conv_u8:
                        expressionStack.Push(CastTo(expressionStack.Pop(), typeof(ulong)));
                        break;
                    case OpCodes.Dup:
                        expressionStack.Push(expressionStack.Peek());
                        break;
                    case OpCodes.Newarr:
                        {
                            var bounds = expressionStack.Pop();
                            var type = instruction.OprandAsType();
                            expressionStack.Push(Expression.NewArrayBounds(type, new[] { bounds }));
                        }
                        break;
                    case OpCodes.Newobj:
                        {
                            var constructor = (ConstructorInfo)instruction.OprandAsMethod();
                            Expression[] args = new Expression[constructor.GetParameters().Length];
                            for(int i = args.Length - 1; i >= 0; i++)
                                args[i] = expressionStack.Pop();
                            expressionStack.Push(Expression.New(constructor, args));
                        }
                        break;
                    case OpCodes.Rethrow:
                        expressionStack.Push(Expression.Rethrow());
                        break;
                    case OpCodes.Tail_:
                        isTailCall = true;
                        break;
                    case OpCodes.Unaligned_:
                        throw new Exception("TODO: unaligned");
                    case OpCodes.Unbox or OpCodes.Unbox_any:
                        {
                            var type = instruction.OprandAsType();
                            var value = expressionStack.Pop();
                            expressionStack.Push(Expression.Unbox(value, type));
                        }
                        break;
                    default:
                        throw new Exception($"Could not find parser for {(OpCodes)instruction.OpCode.Value}");
                        #endregion
                }
            }

            return Expression.Block(expressionStack.Reverse());
        }

        private static Expression CastTo(Expression value, Type targetType)
        {
            return value is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ChangeType(constantExpression.Value, targetType))
                : Expression.Convert(value, targetType);
        }

        private static Expression CastToChecked(Expression value, Type targetType)
        {
            return value is ConstantExpression constantExpression
                ? Expression.Constant(Convert.ChangeType(constantExpression.Value, targetType))
                : Expression.Convert(value, targetType);
        }

        private static Dictionary<int, Label> GetBranches(int position, ref ILReader reader)
        {
            var count = BitConverter.ToInt32(reader.PeekBytes(position, 4));
            var branches = new Dictionary<int, Label>();
            for(int i = 0; i < count; i++)
            {
                var offset = BitConverter.ToInt32(reader.PeekBytes(position + 4 + i * 4, 4));
                branches.Add(offset, new(position + 4 * count + offset));
            }
            return branches;
        }
    }
}
