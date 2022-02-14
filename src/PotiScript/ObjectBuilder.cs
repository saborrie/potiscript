
using PotiScript.Runtime;

namespace PotiScript
{
    public class ObjectBuilder
    {
        private TypeSystem.Object @object;

        public ObjectBuilder()
        {
            this.@object = new TypeSystem.Object();
        }

        public TypeSystem.Object Build() => this.@object;

        public ProxyWriter Add(string key)
        {
            return new ProxyWriter(value =>
            {
                this.@object[key] = value;
            });
        }
    }
}
