using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.ILExpressionParser
{
    internal class ExpressionParser
    {
        public static Expression<T> Parse<T>(Delegate func)
        {
            using var reader = new ILReader(func.Method);
            var expressionStack = new Stack<Expression>();
            
            while(reader.ReadNext(out var instruction))
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
                        break;
                        

                        #endregion
                }
            }

            return null!;
        }
    }
}
