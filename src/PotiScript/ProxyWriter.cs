using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PotiScript.Runtime;

namespace PotiScript
{
    public class ProxyWriter
    {
        private readonly Action<TypeSystem.Object> write;

        public ProxyWriter(Action<TypeSystem.Object> write)
        {
            this.write = write;
        }

        public ProxyWriter Number(decimal value)
        {
            write(new TypeSystem.Number(value));
            return this;
        }

        public ProxyWriter String(string value)
        {
            if (value != null)
            {
                write(new TypeSystem.String(value));
            }
            return this;
        }

        public ProxyWriter Boolean(bool value)
        {
            write(new TypeSystem.Boolean(value));
            return this;
        }

        public ProxyWriter DateTime(DateTime value)
        {
            write(new TypeSystem.DateTime(value));
            return this;
        }

        public ProxyWriter Function(Func<CallContext, CancellationToken, Task> handler)
        {
            write(new TypeSystem.Function(async (args, ct) =>
            {
                var callContext = new CallContext(args);
                await handler(callContext, ct);
                return callContext.ReturnValue;
            }));
            return this;
        }

        public ProxyWriter Object(Action<ObjectBuilder> configure)
        {
            var builder = new ObjectBuilder();
            configure(builder);
            write(builder.Build());
            return this;
        }

        public ProxyWriter Enumerable<T>(IAsyncEnumerable<T> enumerable, Action<EnumerableMapContext<T>> map)
        {
            var x = enumerable.Select(item =>
            {
                var context = new EnumerableMapContext<T>(item);
                map(context);
                return context.ReturnValue ?? new TypeSystem.Null();
            });

            write(new TypeSystem.Enumerable(x));
            return this;
        }
    }
}
