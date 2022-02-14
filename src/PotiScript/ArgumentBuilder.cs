using System.Collections.Generic;

using PotiScript.Runtime;

namespace PotiScript
{
    public class ArgumentBuilder
    {
        private readonly List<TypeSystem.Object> args = new();

        public TypeSystem.Object[] Build()
        {
            return args.ToArray();
        }

        public ProxyWriter Add()
        {
            return new ProxyWriter(value =>
            {
                this.args.Add(value);
            });
        }
    }
}