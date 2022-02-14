
using PotiScript.Runtime;

namespace PotiScript
{
    public class ObjectReader
    {
        private TypeSystem.Object @object;

        public ObjectReader(TypeSystem.Object @object)
        {
            this.@object = @object;
        }

        public ProxyReader Get(string key) => new ProxyReader(() => @object[key]);
    }
}
