
using System;
using System.Collections.Generic;
using System.Linq;
using PotiScript.Runtime;

namespace PotiScript
{
    public class CallContext
    {
        private readonly ProxyReader[] args;
        public TypeSystem.Object ReturnValue { get; private set; }

        public CallContext(TypeSystem.Object[] args)
        {
            this.args = args.Select(x => new ProxyReader(() => x)).ToArray();
            this.ReturnValue = new TypeSystem.Null();
        }

        public IReadOnlyList<ProxyReader> Args => args;

        public ProxyWriter Return => new((value) =>
        {
            this.ReturnValue = value;
        });
    }
}
