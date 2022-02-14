
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using PotiScript.Exceptions;
using PotiScript.Grammar;
using PotiScript.Runtime;

namespace PotiScript
{
    public class PotiScriptInterpreter
    {
        private readonly ReferenceSystem.Scope globalScope;
        private readonly Parser parser;
        private readonly ASTVisitor visitor;
        private readonly StringBuilder log;

        public PotiScriptInterpreter()
        {
            this.globalScope = ReferenceSystem.Scope.Global();
            this.parser = new Parser();
            this.visitor = new ASTVisitor(this.globalScope);
            this.log = new StringBuilder();
            Framework.InstallGlobals(this);
        }

        public void PrintLine(string text)
        {
            this.log.AppendFormat("{0}\n", text);
        }

        public string GetLog()
        {
            return this.log.ToString();
        }

        public ProxyWriter Add(string key)
        {
            return new ProxyWriter(value =>
            {
                this.globalScope.Declare(key);
                this.globalScope[key] = value;
            });
        }

        public ParseResult Validate(string program)
        {
            try
            {
                var ast = this.parser.Parse(program);
                return ParseResult.CreateSuccess();
            }
            catch (SyntaxErrorException ex)
            {
                return ParseResult.CreateError($"Syntax Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ParseResult.CreateError($"Unknown Error: {ex.Message}");
            }
        }

        public async Task<ExecutionResult> ExecAsync(string program, int timeoutInSeconds = 30)
        {
            var timeoutCancellation = new CancellationTokenSource();
            timeoutCancellation.CancelAfter(TimeSpan.FromSeconds(timeoutInSeconds));

            try
            {
                this.log.Clear();
                var ast = parser.Parse(program);
                var result = (await ast.Accept(visitor, timeoutCancellation.Token)).ReadOrConvertToVariable(globalScope);
                return ExecutionResult.CreateSuccess(new ProxyReader(() => result));
            }
            catch (SyntaxErrorException ex)
            {
                OnException?.Invoke(ex);
                return Error($"Syntax Error: {ex.Message}");
            }
            catch (TypeErrorException ex)
            {
                OnException?.Invoke(ex);
                return Error($"Type Error: {ex.Message}");
            }
            catch (RuntimeErrorException ex)
            {
                OnException?.Invoke(ex);
                return Error($"Runtime Error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                OnException?.Invoke(ex);
                return Error($"Execution timed out after {timeoutInSeconds} seconds");
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
                return Error($"Unknown Error: {ex.Message}");
            }

        }

        private ExecutionResult Error(string message)
        {
            this.log.AppendLine(message);
            return ExecutionResult.CreateError(message);
        }

        public delegate void InterpreterExceptionHandler(Exception ex);

        public event InterpreterExceptionHandler? OnException;
    }
}
