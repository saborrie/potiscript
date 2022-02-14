using System;
using System.Collections.Generic;

using PotiScript.Exceptions;

namespace PotiScript.Runtime
{
    public static class ReferenceSystem
    {
        public abstract record Reference()
        {
            public abstract string FullName { get; }

            public abstract TypeSystem.Object Read();

            public abstract TypeSystem.Object Write(TypeSystem.Object value);


            public T As<T>(Scope scope) where T : TypeSystem.Object
            {
                var o = ReadOrConvertToVariable(scope);
                if (o as T == null)
                {
                    throw new TypeErrorException($@"Cannot convert type ""{o.GetType().Name}"" to {nameof(T)}.");
                }

                return (o as T)!;
            }

            public bool Truthy(Scope scope) => As<TypeSystem.Boolean>(scope).Value;

            public virtual bool IsIdentifier => false;

            public virtual Variable ToVariable(Scope scope) => throw new InvalidOperationException("Cannot convert reference to variable.");

            public virtual Member ToMember(Reference parent) => throw new InvalidOperationException("Cannot convert reference to member.");

            public TypeSystem.Object ReadOrConvertToVariable(Scope scope)
            {
                if (this.IsIdentifier)
                {
                    return ToVariable(scope).Read();
                }

                return Read();
            }

            public TypeSystem.Object WriteOrConvertToVariable(Scope scope, TypeSystem.Object value)
            {
                if (this.IsIdentifier)
                {
                    return ToVariable(scope).Write(value);
                }

                return Write(value);
            }
        }

        public class Scope
        {
            public static Scope Global()
            {
                return new Scope();
            }

            public Scope Push()
            {
                return new Scope
                {
                    parent = this
                };
            }

            public Scope Pop()
            {
                if (parent == null)
                {
                    throw new System.Exception("Stack underflow.");
                }

                return parent;
            }

            private Scope? parent;

            private readonly Dictionary<string, TypeSystem.Object> variables = new();

            public void Declare(string key)
            {
                if (variables.ContainsKey(key))
                {
                    throw new RuntimeErrorException(@$"Cannot declare ""{key}"", it already exists.");
                }

                variables[key] = new TypeSystem.Null();
            }

            public TypeSystem.Object this[string key]
            {
                get
                {
                    if (!variables.ContainsKey(key))
                    {
                        if (parent != null)
                        {
                            return parent[key];
                        }

                        throw new RuntimeErrorException($@"Cannot read from variable. ""{key}"" does not exist.");
                    }
                    return variables[key];
                }
                set
                {
                    if (!variables.ContainsKey(key))
                    {
                        if (parent != null)
                        {
                            parent[key] = value;
                            return;
                        }

                        throw new RuntimeErrorException($@"Cannot write to variable. ""{key}"" does not exist.");
                    }
                    variables[key] = value;
                }
            }
        }

        public record Identifier(string Name) : Reference
        {
            public override string FullName => Name;

            public override TypeSystem.Object Read()
            {
                throw new System.InvalidOperationException("Cannot read from an Identifier.");
            }

            public override TypeSystem.Object Write(TypeSystem.Object value)
            {
                throw new System.InvalidOperationException("Cannot write to an Identifier.");
            }

            public override Variable ToVariable(Scope scope)
            {
                return new Variable(scope, Name);
            }

            public override Member ToMember(Reference parent)
            {
                return new Member(parent, Name);
            }

            public override bool IsIdentifier => true;
        }

        public record ValueWrapper(TypeSystem.Object Value) : Reference
        {
            public override string FullName => Value.GetType().Name;

            public override TypeSystem.Object Read()
            {
                return Value;
            }

            public override TypeSystem.Object Write(TypeSystem.Object value)
            {
                throw new System.Exception("Cannot write to a immutable value.");
            }
        }

        public record Variable(Scope Scope, string Name) : Reference
        {
            public override string FullName => Name;

            public override TypeSystem.Object Read()
            {
                return Scope[Name];
            }

            public override TypeSystem.Object Write(TypeSystem.Object value)
            {
                return Scope[Name] = value;
            }
        }

        public record Member(Reference Parent, string Key) : Reference
        {
            public override string FullName => $"{Parent.FullName}.{Key}";

            public override TypeSystem.Object Read()
            {
                var parent = Parent.Read();
                if (parent.IsNull())
                {
                    throw new RuntimeErrorException(@$"Cannot read member ""{Key}"" on null ""{Parent.FullName}"".");
                }
                else if (parent.HasMember(Key))
                {
                    return parent[Key];
                }
                else
                {
                    return new TypeSystem.Null();
                }
            }

            public override TypeSystem.Object Write(TypeSystem.Object value) => Parent.Read()[Key] = value;
        }
    }
}
