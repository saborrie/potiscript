> NOTE: This project is a work in progress

<br>
<p align="center">
  <img width="450" src="./logo.svg">
</p>
<div style="font-size:20px" align="center">A minimal scripting language with a lot of potential</div>
<br>
<br>

# Description

PotiScript is a white-label interpreted language that can be called from C# code. It comes with a minimal framework to get you started and has a simple type system.

You can add your own globals to the PotiScript interpreter before executing a program; giving you the ability to easily create your own Domain Specific Language.

# Basic Examples

The most basic usage of PotiScript is to evaluate a program.

```cs
using PotiScript;

// create a new interpreter
var interpreter = new PotiScriptInterpreter();

// execute the program
var executionResult = await interpreter.ExecAsync("10 - 4");

// read the value as a decimal
decimal answer = executionResult.GetValueAs.Number(); // 6
```

In the example above we are evaluating `10 - 4`: this looks like an expression, but that's because PotiScript allows you to omit the final semicolon of a script. 

The return value of `PotiScriptInterpreter.ExecAsync` is the value of the last statement in the program. For example, the following program evaluates to the decimal value `4`:

```cs
var executionResult = await interpreter.ExecAsync(@"
'one';
2;
'three';
4
");
```

PotiScript has basic language features like functions, if/else, loops, 

```cs
var executionResult = await interpreter.ExecAsync(@"
def factorial(n) {
  if (n <= 1) return 1;
  return factorial(n - 1) * n;
}

factorial(10);
");

```

## Adding Globals

The PotiScript interpreter gives you the ability to define globals before you call `ExecAsync`. You can use this to parameterise your programs:

```cs
var interpreter = new PotiScriptInterpreter();

// create a variable "input" with decimal value `10`
interpreter.Add("input").Number(10);

var program = @"
def factorial(n) {
  if (n <= 1) return 1;
  return factorial(n - 1) * n;
}

factorial(input);
";

var executionResult = await interpreter.ExecAsync(program);

```

PotiScript can also call into C# functions:

```cs
var interpreter = new PotiScriptInterpreter();

// add global function "sayHello()"
interpreter.Add("sayHello").Function(async () => {
  Console.WriteLine("hello");
});

var program = @"
sayHello();
";

// writes "hello" to the console
await interpreter.ExecAsync(program);
```

# Documentation

TODO