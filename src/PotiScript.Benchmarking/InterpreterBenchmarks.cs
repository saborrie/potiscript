using BenchmarkDotNet.Attributes;

namespace PotiScript.Benchmarking;

[MemoryDiagnoser]
public class InterpreterBenchmarks
{
    private readonly PotiScriptInterpreter interpreter;
    private const string program = @"
def factorial(x) {
  if (x <= 1) return 1;
  return factorial(x - 1) * x;
}

factorial(10);
";

    public InterpreterBenchmarks()
    {
        this.interpreter = new PotiScriptInterpreter();
    }

    [Benchmark]
    public async Task InterpreterFactorial()
    {
        var result = await this.interpreter.ExecAsync(program);
        result.GetValueAs.Number();
    }

    [Benchmark]
    public void CsharpFactorial()
    {
        static int factorial(int x)
        {
            if (x <= 1) return 1;
            return factorial(x - 1) * x;
        }

        _ = factorial(10);
    }

}
