
using Newtonsoft.Json;

using PotiScript.Grammar;

using Xunit;

namespace PotiScript.UnitTests
{
    public static class ParserTestHelpers
    {
        public static void AssertAST(AST.Node expected, string program)
        {
            var sut = new Parser();
            var ast = sut.Parse(program);

            var expectedAsJson = JsonConvert.SerializeObject(expected);
            var astAsJson = JsonConvert.SerializeObject(ast);

            Assert.Equal(expectedAsJson, astAsJson);
        }
    }
}
