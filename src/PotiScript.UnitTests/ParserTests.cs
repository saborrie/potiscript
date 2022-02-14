
using PotiScript.Grammar;

using Xunit;

namespace PotiScript.UnitTests
{
    public class ParserTests
    {
        [Fact]
        public void Parse_WhenGivenANumericLiteral_ProducesValidAST()
        {
            var program = @"12;""p"";";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.NumericLiteral
                        (
                            "12",
                            12
                        )
                    ),

                    new AST.ExpressionStatement(
                        new AST.StringLiteral
                        (
                            "p"
                        )
                    ),
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenNestedBlocks_ProducesValidAST()
        {
            var program = @"{ 42; { ""hello""; } }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.BlockStatement
                    (
                        new AST.NodeList
                        {
                            new AST.ExpressionStatement
                            (
                                new AST.NumericLiteral
                                (
                                    "42",
                                    42
                                )
                            ),
                            new AST.BlockStatement
                            (
                                new AST.NodeList
                                {
                                    new AST.ExpressionStatement
                                    (
                                        new AST.StringLiteral
                                        (
                                            "hello"
                                        )
                                    ),
                                }

                            )
                        }


                    ),


                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenNestedBlocksAndEmpty_ProducesValidAST()
        {
            var program = @"{ 42; { ""hello""; } ; ; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.BlockStatement
                    (
                        new AST.NodeList
                        {
                            new AST.ExpressionStatement
                            (
                                new AST.NumericLiteral
                                (
                                    "42",
                                    42
                                )
                            ),
                            new AST.BlockStatement
                            (
                                new AST.NodeList
                                {
                                    new AST.ExpressionStatement
                                    (
                                        new AST.StringLiteral
                                        (
                                            "hello"
                                        )
                                    ),
                                }

                            ),
                            new AST.EmptyStatement(),
                            new AST.EmptyStatement()
                        }
                    ),


                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAnAdditiveExpression_ProducesValidAST()
        {

            var program = "12 + 16;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.BinaryExpression
                        (
                            "+",
                            new AST.NumericLiteral("12", 12),
                            new AST.NumericLiteral("16", 16)
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAChainedAdditiveExpression_ProducesValidAST()
        {

            var program = "12 + 16 + 2;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.BinaryExpression
                        (
                            "+",
                            new AST.BinaryExpression
                            (
                                "+",
                                new AST.NumericLiteral("12", 12),
                                new AST.NumericLiteral("16", 16)
                            ),
                            new AST.NumericLiteral("2", 2)
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAChainedAdditiveAndMultiplicativeExpression_ProducesValidAST()
        {

            var program = "12 + 16 * 2;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.BinaryExpression
                        (
                            "+",
                            new AST.NumericLiteral("12", 12),
                            new AST.BinaryExpression
                            (
                                "*",
                                new AST.NumericLiteral("16", 16),
                                new AST.NumericLiteral("2", 2)
                            )
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenABracketedChainedAdditiveAndMultiplicativeExpression_ProducesValidAST()
        {

            var program = "(12 + 16) * 2;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.BinaryExpression
                        (
                            "*",
                            new AST.BinaryExpression
                            (
                                "+",
                                new AST.NumericLiteral("12", 12),
                                new AST.NumericLiteral("16", 16)
                            ),
                            new AST.NumericLiteral("2", 2)
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenASimpleAssign_ProducesValidAST()
        {
            var program = @"x = 1;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.AssignmentExpression
                        (
                            "=",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("1", 1)
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAChainedSimpleAssign_ProducesValidAST()
        {
            var program = @"x = y = 1;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.AssignmentExpression
                        (
                            "=",
                            new AST.Identifier("x"),
                            new AST.AssignmentExpression
                            (
                                "=",
                                new AST.Identifier("y"),
                                new AST.NumericLiteral("1", 1)
                            )
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAVariableStatement_ProducesValidAST()
        {
            var program = @"let x = 1;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.VariableStatement(
                        new AST.NodeList
                        {
                            new AST.VariableDeclaration(
                                new AST.Identifier("x"),
                                new AST.NumericLiteral("1", 1)
                            )
                        }
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAVariableStatementWithNoInit_ProducesValidAST()
        {
            var program = @"let x;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.VariableStatement(
                        new AST.NodeList
                        {
                            new AST.VariableDeclaration(
                                new AST.Identifier("x")
                            )
                        }
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }


        [Fact]
        public void Parse_WhenGivenAnIfElseStatement_ProducesValidAST()
        {
            var program = @"if (x) { x = 1; } else { x = 2; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.IfStatement(
                        new AST.Identifier("x"),
                        new AST.BlockStatement(
                            new AST.NodeList {
                                new AST.ExpressionStatement(
                                    new AST.AssignmentExpression(
                                        "=",
                                        new AST.Identifier("x"),
                                        new AST.NumericLiteral("1", 1)
                                    )
                                )
                            }
                        ),
                        new AST.BlockStatement(
                            new AST.NodeList {
                                new AST.ExpressionStatement(
                                    new AST.AssignmentExpression(
                                        "=",
                                        new AST.Identifier("x"),
                                        new AST.NumericLiteral("2", 2)
                                    )
                                )
                            }
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAnIfStatement_ProducesValidAST()
        {
            var program = @"if (x) { x = 1; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.IfStatement(
                        new AST.Identifier("x"),
                        new AST.BlockStatement(
                            new AST.NodeList {
                                new AST.ExpressionStatement(
                                    new AST.AssignmentExpression(
                                        "=",
                                        new AST.Identifier("x"),
                                        new AST.NumericLiteral("1", 1)
                                    )
                                )
                            }
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenARelationalExpression_ProducesValidAST()
        {
            var program = @"x > 0;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.BinaryExpression(
                            ">",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("0", 0)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAnEqualityExpression_ProducesValidAST()
        {
            var program = @"x == 5;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.BinaryExpression(
                            "==",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("5", 5)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAnEqualityExpressionWithBool_ProducesValidAST()
        {
            var program = @"x == false;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.BinaryExpression(
                            "==",
                            new AST.Identifier("x"),
                            new AST.BooleanLiteral(false)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenALogicalExpression_ProducesValidAST()
        {
            var program = @"x && false;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.LogicalExpression(
                            "&&",
                            new AST.Identifier("x"),
                            new AST.BooleanLiteral(false)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenANullPropagatingExpression_ProducesValidAST()
        {
            var program = @"x ?? false;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.LogicalExpression(
                            "??",
                            new AST.Identifier("x"),
                            new AST.BooleanLiteral(false)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAUnaryExpression_ProducesValidAST()
        {
            var program = @"!true;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.UnaryExpression(
                            "!",
                            new AST.BooleanLiteral(true)
                        )
                    ),
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenALoop_ProducesValidAST()
        {
            var program = @"while(x < 10) { 5; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.WhileStatement
                    (
                        new AST.BinaryExpression
                        (
                            "<",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("10", 10)
                        ),
                        new AST.BlockStatement
                        (
                            new AST.NodeList
                            {
                                new AST.ExpressionStatement
                                (
                                    new AST.NumericLiteral("5", 5)
                                )
                            }
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenADoWhileLoop_ProducesValidAST()
        {
            var program = @"do { 5; } while (x < 10);";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.DoWhileStatement
                    (
                        new AST.BlockStatement
                        (
                            new AST.NodeList
                            {
                                new AST.ExpressionStatement
                                (
                                    new AST.NumericLiteral("5", 5)
                                )
                            }
                        ),
                        new AST.BinaryExpression
                        (
                            "<",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("10", 10)
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAForLoop_ProducesValidAST()
        {
            var program = @"for(let x =0; x < 10; x += 1) { 5; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ForStatement
                    (
                        new AST.VariableStatement
                        (
                            new AST.NodeList
                            {
                                new AST.VariableDeclaration
                                (
                                    new AST.Identifier("x"),
                                    new AST.NumericLiteral("0", 0)
                                )
                            }
                        ),
                        new AST.BinaryExpression
                        (
                            "<",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("10", 10)
                        ),
                        new AST.AssignmentExpression
                        (
                            "+=",
                            new AST.Identifier("x"),
                            new AST.NumericLiteral("1", 1)
                        ),
                        new AST.BlockStatement
                        (
                            new AST.NodeList
                            {
                                new AST.ExpressionStatement
                                (
                                    new AST.NumericLiteral("5", 5)
                                )
                            }
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAFunctionDeclaration_ProducesValidAST()
        {
            var program = @"def funky(x, y) { return 5; }";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.FunctionDeclaration
                    (
                        new AST.Identifier("funky"),
                        new AST.NodeList
                        {
                            new AST.Identifier("x"),
                            new AST.Identifier("y")
                        },
                        new AST.BlockStatement
                        (
                            new AST.NodeList
                            {
                                new AST.ReturnStatement
                                (
                                    new AST.NumericLiteral("5", 5)
                                )
                            }
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAMemberExpression_ProducesValidAST()
        {
            var program = @"a.b.c;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.MemberExpression(
                            false,
                            false,
                            new AST.MemberExpression(
                                false,
                                false,
                                new AST.Identifier("a"),
                                new AST.Identifier("b")
                            ),
                            new AST.Identifier("c")
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAComputedMemberExpression_ProducesValidAST()
        {
            var program = @"a[""Bee""].c;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement
                    (
                        new AST.MemberExpression(
                            false,
                            false,
                            new AST.MemberExpression(
                                false,
                                true,
                                new AST.Identifier("a"),
                                new AST.StringLiteral("Bee")
                            ),
                            new AST.Identifier("c")
                        )
                    )
                }
            );
            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenASimpleAssignToMember_ProducesValidAST()
        {
            var program = @"x.y = 1;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.AssignmentExpression
                        (
                            "=",
                            new AST.MemberExpression(
                                false,
                                false,
                                new AST.Identifier("x"),
                                new AST.Identifier("y")
                            ),
                            new AST.NumericLiteral("1", 1)
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenANullPropagation_ProducesValidAST()
        {
            var program = @"x?.y;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.MemberExpression(
                            true,
                            false,
                            new AST.Identifier("x"),
                            new AST.Identifier("y")
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenACallExpression_ProducesValidAST()
        {
            var program = @"foo(x)();";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.CallExpression
                        (
                            new AST.CallExpression(
                                new AST.Identifier("foo"),
                                new AST.NodeList
                                {
                                    new AST.Identifier("x")

                                }
                            ),
                            new AST.NodeList
                            {
                            }
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAPercentage_ProducesValidAST()
        {
            var program = @"23%;";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.NumericLiteral("23%", 0.23m)
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenATemplateLiteral_ProducesValidAST()
        {
            var program = @"$""Hello"";";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.TemplateLiteral(
                            new AST.NodeList
                            {
                                new AST.PartialString("Hello")
                            }
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAComplexTemplateLiteral_ProducesValidAST()
        {
            var program = @"$""3 bananas plus 4 bananas is {3 + 4} bananas."";";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(
                        new AST.TemplateLiteral(
                            new AST.NodeList
                            {
                                new AST.PartialString("3 bananas plus 4 bananas is "),
                                new AST.BinaryExpression("+", new AST.NumericLiteral("3", 3m), new AST.NumericLiteral("4", 4m)),
                                new AST.PartialString(" bananas.")
                            }
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAForeachStatement_ProducesValidAST()
        {
            var program = @"foreach(let i in items) write(i);";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ForeachStatement(
                        new AST.Identifier("i"),
                        new AST.Identifier("items"),
                        new AST.ExpressionStatement(
                            new AST.CallExpression(new AST.Identifier("write"), new AST.NodeList {
                                new AST.Identifier("i")
                            })
                        )
                    )
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }

        [Fact]
        public void Parse_WhenGivenAProgramWithAnImplicitSemicolon_ProducesValidAST()
        {
            var program = @"a; b; c()";
            var expected = new AST.Program
            (
                new AST.NodeList
                {
                    new AST.ExpressionStatement(new AST.Identifier("a")),
                    new AST.ExpressionStatement(new AST.Identifier("b")),
                    new AST.ExpressionStatement(new AST.CallExpression(new AST.Identifier("c"), new AST.NodeList()))
                }
            );

            ParserTestHelpers.AssertAST(expected, program);
        }
    }
}
