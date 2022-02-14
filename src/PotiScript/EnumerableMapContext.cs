
using PotiScript.Runtime;

namespace PotiScript
{
    public class EnumerableMapContext<T>
    {
        public EnumerableMapContext(T input)
        {
            this.Input = input;
        }

        public ProxyWriter Return => new ProxyWriter((value) =>
        {
            this.ReturnValue = value;
        });

        public TypeSystem.Object? ReturnValue { get; private set; }
        public T Input { get; }
    }
}
