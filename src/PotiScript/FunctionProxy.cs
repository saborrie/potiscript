
using System;
using System.Threading;
using System.Threading.Tasks;

using PotiScript.Exceptions;
using PotiScript.Runtime;

namespace PotiScript
{
    public class FunctionProxy
    {
        private TypeSystem.Function function;

        public FunctionProxy(TypeSystem.Function function)
        {
            this.function = function;
        }

        public async Task<ExecutionResult> CallAsync(Action<ArgumentBuilder> configureArgs, CancellationToken ct)
        {
            try
            {
                var argumentBuilder = new ArgumentBuilder();
                configureArgs(argumentBuilder);
                var result = await function.Call(argumentBuilder.Build(), ct);
                return ExecutionResult.CreateSuccess(new ProxyReader(() => result));
            }
            catch (SyntaxErrorException ex)
            {
                return ExecutionResult.CreateError($"Syntax Error: {ex.Message}");
            }
            catch (TypeErrorException ex)
            {
                return ExecutionResult.CreateError($"Type Error: {ex.Message}");
            }
            catch (RuntimeErrorException ex)
            {
                return ExecutionResult.CreateError($"Runtime Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ExecutionResult.CreateError($"Unknown Error: {ex.Message}");
            }
        }
    }
}
