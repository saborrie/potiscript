using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using PotiScript.Exceptions;

namespace PotiScript.Runtime
{
    public static class TypeSystem
    {
        public record Object
        {
            protected Dictionary<string, Object> Members = new();

            public Object()
            {
                Framework.InstallExtensions(
                    (dynamic)this,
                    (Func<string, ProxyWriter>)(key => new ProxyWriter(x => Members.Add(key, x)))
                );
            }

            public virtual Object this[string key]
            {
                get
                {
                    ThrowIfNull(@$"Cannot get property ""{key}"" on null.");

                    if (!Members.ContainsKey(key))
                    {
                        throw new KeyNotFoundException(key);
                    }
                    return Members[key];
                }
                set
                {
                    ThrowIfNull(@$"Cannot set property ""{key}"" on null.");

                    Members[key] = value;
                }
            }

            private void ThrowIfNull(string reason)
            {
                if (IsNull())
                {
                    throw new TypeErrorException(reason);
                }
            }

            public bool HasMember(string key) => Members.ContainsKey(key);

            public virtual Object UnaryOperation(string @operator)
            {
                throw new RuntimeErrorException(@$"Cannot apply operator ""{@operator}"" to type {this.GetType().Name}");
            }

            public virtual Object BinaryOperation(string @operator, Object operand)
            {
                if (@operator == "==" && operand.GetType() == typeof(Null))
                {
                    return new Boolean(this.GetType() == typeof(Null));
                }
                else if (@operator == "!=" && operand.GetType() == typeof(Null))
                {
                    return new Boolean(this.GetType() != typeof(Null));
                }
                else if (@operator == "??")
                {
                    return !IsNull() ? this : operand;
                }

                throw new RuntimeErrorException(@$"Cannot apply operator ""{@operator}"" to types {this.GetType().Name} and {operand.GetType().Name}");
            }

            public virtual string ConvertToString()
            {
                throw new TypeErrorException(@$"Cannot convert {this.GetType().Name} to String");
            }

            public bool IsNull()
            {
                return this.GetType() == typeof(Null);
            }
        }

        public record ImmutableObject : Object
        {
            public override Object this[string key]
            {
                get => base[key];

                // TODO pass exceptions back nicely
                set => throw new System.Exception("Cannot set value on immutable object.");
            }
        }

        public record String(string Value) : ImmutableObject
        {
            public override Object BinaryOperation(string @operator, Object operand)
            {
                if (operand.GetType() != typeof(String))
                {
                    return base.BinaryOperation(@operator, operand);
                }

                var x = (operand as String)!.Value;

                return (@operator) switch
                {
                    "+" => new String(Value + x),
                    "==" => new Boolean(Value.Equals(x)),
                    "!=" => new Boolean(!Value.Equals(x)),
                    _ => base.BinaryOperation(@operator, operand)
                };
            }

            public override string ConvertToString()
            {
                return Value;
            }
        }

        public record Null() : ImmutableObject
        {
            public override string ConvertToString()
            {
                return string.Empty;
            }
        }

        public record Boolean(bool Value) : ImmutableObject
        {
            public override Object UnaryOperation(string @operator)
            {
                return (@operator) switch
                {
                    "!" => new Boolean(!Value),
                    _ => base.UnaryOperation(@operator)
                };
            }

            public override Object BinaryOperation(string @operator, Object operand)
            {
                if (operand.GetType() != typeof(Boolean))
                {
                    return base.BinaryOperation(@operator, operand);
                }

                var x = (operand as Boolean)!.Value;

                return (@operator) switch
                {
                    "&&" => new Boolean(Value && x),
                    "||" => new Boolean(Value || x),
                    _ => base.BinaryOperation(@operator, operand)
                };
            }
        }


        public record Number(decimal Value) : ImmutableObject
        {
            public override Object UnaryOperation(string @operator)
            {
                return (@operator) switch
                {
                    "-" => new Number(-Value),
                    "+" => this,
                    _ => base.UnaryOperation(@operator)
                };
            }

            public override Object BinaryOperation(string @operator, Object operand)
            {
                if (operand.GetType() != typeof(Number))
                {
                    return base.BinaryOperation(@operator, operand);
                }

                var x = (operand as Number)!.Value;

                return (@operator) switch
                {
                    // maths
                    "+" => new Number(Value + x),
                    "-" => new Number(Value - x),
                    "*" => new Number(Value * x),
                    "/" => new Number(Value / x),

                    // comparisons
                    "<" => new Boolean(Value < x),
                    "<=" => new Boolean(Value <= x),
                    ">" => new Boolean(Value > x),
                    ">=" => new Boolean(Value >= x),
                    "==" => new Boolean(Value == x),
                    "!=" => new Boolean(Value != x),

                    _ => base.BinaryOperation(@operator, operand)
                };
            }

            public override string ConvertToString()
            {
                return Convert.ToString(Value);
            }
        }
        public record DateTime(System.DateTime Value) : ImmutableObject
        {
            public override Object BinaryOperation(string @operator, Object operand)
            {
                if (operand.GetType() != typeof(DateTime))
                {
                    return base.BinaryOperation(@operator, operand);
                }

                var x = (operand as DateTime)!.Value;

                return (@operator) switch
                {
                    // comparisons
                    "<" => new Boolean(Value < x),
                    "<=" => new Boolean(Value <= x),
                    ">" => new Boolean(Value > x),
                    ">=" => new Boolean(Value >= x),
                    "==" => new Boolean(Value == x),
                    "!=" => new Boolean(Value != x),

                    _ => base.BinaryOperation(@operator, operand)
                };
            }

            public override string ConvertToString()
            {
                return Convert.ToString(Value);
            }
        }
        public record Function(Func<Object[], CancellationToken, Task<Object>> Call) : ImmutableObject;

        public record Enumerable(IAsyncEnumerable<Object> Value) : ImmutableObject, IAsyncEnumerable<Object>
        {
            public IAsyncEnumerator<Object> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return Value.GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}
