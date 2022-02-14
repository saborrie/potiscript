using System;

using PotiScript.Runtime;

namespace PotiScript
{
    public class ExecutionResult
    {
        public ExecutionResult(ProxyReader reader)
        {
            this.IsSuccess = true;
            this.GetValueAs = reader;
        }

        public ExecutionResult(string error)
        {
            this.IsSuccess = false;
            this.Error = error;
            this.GetValueAs = ProxyReader.Null;
        }

        public bool IsSuccess { get; private set; }

        public ProxyReader GetValueAs { get; private set; }

        public string? Error { get; private set; }


        public static ExecutionResult CreateSuccess(Action<ProxyWriter> write)
        {
            TypeSystem.Object? value = null;
            var proxyWriter = new ProxyWriter(x => value = x);
            write(proxyWriter);
            return CreateSuccess(new ProxyReader(() => value));
        }

        public static ExecutionResult CreateSuccess(ProxyReader reader) =>
            new(reader);

        public static ExecutionResult CreateError(string error) =>
            new(error);
    }
}
