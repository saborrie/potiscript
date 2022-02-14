using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace PotiScript.UnitTests
{
    public class InterpreterTests
    {
        private readonly ITestOutputHelper output;

        public InterpreterTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAValidFunctionCallThatReturnsANumber_ReturnsTheCorrectNumber()
        {
            var interpreter = new PotiScriptInterpreter();
            var program = "let x = 5; def sayten(a) { a * x; } sayten(2);";
            var result = await interpreter.ExecAsync(program);
            var expected = 10m;

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAProgramThatReturnsNull_ReturnsNull()
        {
            var interpreter = new PotiScriptInterpreter();
            var program = "null;";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Null(result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenStringLengthCall_ReturnsTheCorrectLength()
        {
            var interpreter = new PotiScriptInterpreter();
            var program = @"""1234"".length;";
            var result = await interpreter.ExecAsync(program);
            var expected = 4m;

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAGlobalFunction_CallsItCorrectly()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("getTen").Function((call, ct) => {
                call.Return.Number(10m);
                return Task.CompletedTask;
            });

            var program = "getTen();";
            var expected = 10m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAGlobalFunction_AcceptsArgumentsCorrectly()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("addThenMultiplyByTen").Function((call, ct) => {
                var x1 = call.Args[0].Number() ?? 0m;
                var x2 = call.Args[1].Number() ?? 0m;

                call.Return.Number((x1 + x2) * 10m);
                return Task.CompletedTask;
            });

            var program = "addThenMultiplyByTen(5, 6);";
            var expected = 110m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAGlobalFunctionAsAMember_AcceptsArgumentsCorrectly()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("math").Object(math =>
                {
                    math.Add("addThenMultiplyByTen").Function((call, ct) => {
                        var x1 = call.Args[0].Number() ?? 0m;
                        var x2 = call.Args[1].Number() ?? 0m;

                        call.Return.Number((x1 + x2) * 10m);
                        return Task.CompletedTask;
                    });
                });


            var program = "math.addThenMultiplyByTen(5, 6);";
            var expected = 110m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAGlobalFunctionAsAComputedAMember_AcceptsArgumentsCorrectly()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("math").Object(math =>
            {
                math.Add("addThenMultiplyByTen").Function((call, ct) => {
                    var x1 = call.Args[0].Number() ?? 0m;
                    var x2 = call.Args[1].Number() ?? 0m;

                    call.Return.Number((x1 + x2) * 10m);
                    return Task.CompletedTask;
                });
            });


            var program = @"math[""addThenMultiplyByTen""](5, 6);";
            var expected = 110m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAFunctionThatReturnsAnObject_IsAbleToCallMembersOfTheObject()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("createPiFactory").Function((call, ct) => {
                call.Return.Object(o =>
               {
                   o.Add("getPi").Function((call, ct) => {
                       call.Return.Number(3.14m);
                       return Task.CompletedTask;
                   });
               });
                return Task.CompletedTask;
            });

            var program = @"createPiFactory().getPi();";
            var expected = 3.14m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAFunctionThatReturnsAFunction_IsAbleToCallTheFunction()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("createPiFunction").Function((call, ct) => {
                call.Return.Function((call, ct) => {
                    call.Return.Number(3.14m);
                    return Task.CompletedTask;
                });
                return Task.CompletedTask;
            });

            var program = @"createPiFunction()();";
            var expected = 3.14m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecDateTime_WhenGivenNow_ReturnsCorrectDateTime()
        {
            var now = System.DateTime.UtcNow;

            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("now").Function((call, ct) => {
                call.Return.DateTime(now);
                return Task.CompletedTask;
            });

            var program = @"now();";
            var expected = now;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.DateTime());
        }

        [Fact]
        public async Task ExecDateTime_WhenGivenNowAddDays_ReturnsCorrectDateTime()
        {
            var now = System.DateTime.UtcNow;

            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("now").Function((call, ct) => {
                call.Return.DateTime(now);
                return Task.CompletedTask;
            });

            var program = @"now().addDays(4);";
            var expected = now.AddDays(4);

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.DateTime());
        }

        [Fact]
        public async Task ExecNumber_WhenGivenAnObjectAssignment_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("object").Function((call, ct) => {
                call.Return.Object(o =>
               {
               });
                return Task.CompletedTask;
            });

            var program = @"let x = object(); x.y = 10; let z = x.y; z * 2;";
            var expected = 20m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenAnAsyncFunction_AwaitsTheResult()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("query").Function(async (call, ct) =>
            {
                await Task.Delay(1000, ct);

                call.Return.Number(2m);
            });

            var program = @"query();";
            var expected = 2m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenATemplateLiteral_ReturnsAValidString()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"def getWorld() { ""world""; } $""Hello {getWorld()}"";";
            var expected = "Hello world";

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenATemplateLiteralTree_ReturnsAValidString()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"def getWorld() { ""world""; } $""Hello {$""foo {getWorld()}""}"";";
            var expected = "Hello foo world";

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenACheckWithStringEqualityWorksAsExpecte()
        {
            var interpreter = new PotiScriptInterpreter();

            var program1 = @"""a"" == ""b"";";
            var expected1 = false;

            var result1 = await interpreter.ExecAsync(program1);

            Assert.Null(result1.Error);
            Assert.Equal(expected1, result1.GetValueAs.Boolean());

            var program2 = @"""a"" == ""a"";";
            var expected2 = true;

            var result2 = await interpreter.ExecAsync(program2);

            Assert.Null(result2.Error);
            Assert.Equal(expected2, result2.GetValueAs.Boolean());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenAForeachOverEnumerator_ReturnsAValidResult()
        {
            static IEnumerable<decimal> GenerateList()
            {
                yield return 5m;
                yield return 7m;
            }

            var interpreter = new PotiScriptInterpreter();

            interpreter.Add("items").Enumerable(GenerateList().ToAsyncEnumerable(), map =>
            {
                map.Return.Number(map.Input);
            });
            interpreter.Add("write").Function((call, ct) => {
                var input = call.Args[0].Number();
                call.Return.Number(input!.Value + 10m);
                return Task.CompletedTask;
            });

            var program = @"foreach(let i in items) write(i);";
            var expected = 17m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenASingleVariableIfTest_ReturnsAValidResult()
        {

            var interpreter = new PotiScriptInterpreter();

            interpreter.Add("aTruthyValue").Boolean(true);

            var program = @"if(aTruthyValue) { 1; } else { 2; }";
            var expected = 1m;

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenAFunctionThatReturnsAnEnumerable_ReturnsAValidResult()
        {
            var interpreter = new PotiScriptInterpreter();

            interpreter.Add("query").Function((call, ct) => {
                async IAsyncEnumerable<Dictionary<string, string>> LoadData()
                {
                    // e.g. load data from an external source: var results = service.Query()...
                    await Task.Delay(1, ct);

                    // e.g. foreach over the results set, then yield return each item
                    yield return new Dictionary<string, string>
                    {
                        ["item"] = "a"
                    };

                    yield return new Dictionary<string, string>
                    {
                        ["item"] = "b"
                    };
                }

                call.Return.Enumerable(LoadData(), x =>
                {
                    // load the input item from the enumerable "a", "b"
                    var inputItem = x.Input["item"];

                    // tell the enumerable to map to an object
                    x.Return.Object(o =>
                    {
                        // define an 'item' property on that object
                        o.Add("item").String(inputItem);
                    });
                });

                return Task.CompletedTask;
            });


            var program = @"let results = query(); let output = """"; foreach(let result in results) { output += result.item; } output;";
            var expected = "ab";

            interpreter.OnException += (ex) =>
            {
                output.WriteLine(ex.Message);
                output.WriteLine(ex.StackTrace);
            };

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(expected, result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenPrints_UpdatesTheLog()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"print(""hello""); print(""world"");";

            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("hello\nworld\n", interpreter.GetLog());
        }


        [Fact]
        public async Task ExecAsync_WhenGivenAFunctionWhichTakesMoreThanTimeout_TimesOut()
        {
            var interpreter = new PotiScriptInterpreter();
            interpreter.Add("longFunc").Function(async (call, ct) =>
            {
                await Task.Delay(4000, ct);

                call.Return.Number(2m);
            });

            var program = @"longFunc();";
            var result = await interpreter.ExecAsync(program, 1);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Error);
            Assert.StartsWith("Execution timed out", result.Error);
        }

        [Fact]
        public async Task ExecAsync_WhenGivenNullEqualsNull_ReturnsTrue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"null == null;";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.True(result.GetValueAs.Boolean());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenNullNotEqualsNull_ReturnsFalse()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"null != null;";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.False(result.GetValueAs.Boolean());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenNullInTemplateString_PrintsEmpty()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"$""hello-{null}-world"";";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("hello--world", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenNullCoalescing_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"let a; a ?? 100;";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(100m, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenGivenNullPropagating_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"let a; a?.b ?? 100;";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(100m, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenCallingStringFormat_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"string.format(""{0:00000000}"", 2342);";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("00002342", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenCallingStringFormatDateTime_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            interpreter.Add("x").DateTime(new System.DateTime(2021, 10, 1));

            var program = @"string.format(""{0:dd/MM/yyyy}"", x);";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("01/10/2021", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenCallingStringGetDigits_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"string.getDigits(""ABC000123456"");";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("000123456", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenCallingConvertToNumber_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"convert.toNumber(""1312342.1234"");";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(1312342.1234m, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenFormattingAlphaNumericToNumber_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"string.format(""{0:00000000}"", convert.toNumber(string.getDigits(""ASD002759"")));";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("00002759", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGettingSubstring_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"""John Doe"".substring(0, 1);";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("J", result.GetValueAs.String());
        }

        [Fact]
        public async Task ExecAsync_WhenGettingLastIndexOf_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"""John Doe"".lastIndexOf("" "");";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(4, result.GetValueAs.Number());
        }

        [Fact]
        public async Task ExecAsync_WhenExtractingSurname_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            interpreter.Add("name").String("John Doe");

            var program = @"let index = name.lastIndexOf("" ""); name.substring(index + 1, name.length - index - 1);";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal("Doe", result.GetValueAs.String());
        }


        [Fact]
        public async Task ExecAsync_WhenGivenExpressionStatementWithImplicitSemicolon_ReturnsCorrectValue()
        {
            var interpreter = new PotiScriptInterpreter();

            var program = @"10 - 4";
            var result = await interpreter.ExecAsync(program);

            Assert.Null(result.Error);
            Assert.Equal(6m, result.GetValueAs.Number());
        }

    }
}
