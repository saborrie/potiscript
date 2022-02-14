using System;

using PotiScript.Runtime;

namespace PotiScript
{
    public class ProxyReader
    {
        public static readonly ProxyReader Null = new(() => null);
        
        private readonly Func<TypeSystem.Object?> read;

        public ProxyReader(Func<TypeSystem.Object?> read)
        {
            this.read = read;
        }

        public decimal? Number() => (read() as TypeSystem.Number)?.Value;
        public string? String() => (read() as TypeSystem.String)?.Value;
        public DateTime? DateTime() => (read() as TypeSystem.DateTime)?.Value;
        public bool? Boolean() => (read() as TypeSystem.Boolean)?.Value;

        public decimal? ConvertToNumber() => Convert.ToDecimal(Primitive());
        public string? ConvertToString() => Convert.ToString(Primitive());
        public DateTime? ConvertToDateTime() => Convert.ToDateTime(Primitive());
        public bool? ConvertToBoolean() => Convert.ToBoolean(Primitive());

        public object? Primitive()
        {
            var output = read();
            if (output is TypeSystem.Number) return (output as TypeSystem.Number)?.Value;
            if (output is TypeSystem.String) return (output as TypeSystem.String)?.Value;
            if (output is TypeSystem.DateTime) return (output as TypeSystem.DateTime)?.Value;
            if (output is TypeSystem.Boolean) return (output as TypeSystem.Boolean)?.Value;
            return null;
        }

        public ObjectReader? Object()
        {
            var x = read();

            if (x == null)
            {
                return null;
            }

            return new ObjectReader(x);
        }

        public FunctionProxy? Function()
        {
            var x = read();

            if (x?.GetType() == typeof(TypeSystem.Function))
            {
                return new FunctionProxy((x as TypeSystem.Function)!);
            }
            return null;
        }
    }
}
