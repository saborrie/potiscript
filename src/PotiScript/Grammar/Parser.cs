using System;

using PotiScript.Exceptions;

namespace PotiScript.Grammar
{
    public class Parser
    {


        private Token? lookAhead;
        private readonly Tokeniser tokeniser;

        public Parser()
        {
            this.tokeniser = new Tokeniser();
        }

        public AST.Node Parse(string text)
        {
            tokeniser.Init(text);
            this.lookAhead = tokeniser.GetNextToken();
            return Program();
        }

        public AST.Node ParseExpression(string text)
        {
            tokeniser.Init(text);
            this.lookAhead = tokeniser.GetNextToken();
            return Expression();
        }

        private AST.Node Program()
        {
            return new AST.Program(StatementList());
        }

        private AST.Node Literal()
        {
            return (lookAhead?.Type) switch
            {
                TokenType.String => StringLiteral(),
                TokenType.Number => NumericLiteral(),
                TokenType.True => BooleanLiteral(TokenType.True),
                TokenType.False => BooleanLiteral(TokenType.False),
                TokenType.Null => NullLiteral(),
                _ => throw CreateSyntaxError("Literal: unexpected literal production"),
            };
        }

        private AST.NodeList StatementList(TokenType? stopLookahead = null)
        {
            var statementList = new AST.NodeList();

            while (this.lookAhead?.Type != stopLookahead)
            {
                statementList.Add(Statement());
            }

            return statementList;
        }

        private AST.Node Statement()
        {
            if (lookAhead == null)
            {
                throw CreateSyntaxError(@$"Unexpected end of input: expected a statement.");
            }

            return (lookAhead?.Type) switch
            {
                TokenType.If => IfStatement(),
                TokenType.While or TokenType.Do or TokenType.Foreach or TokenType.For => IterationStatement(),
                TokenType.Def => FunctionDeclaration(),
                TokenType.Return => ReturnStatement(),
                TokenType.Let => VariableStatement(),
                TokenType.Semicolon => EmptyStatement(),
                TokenType.OpenCurly => BlockStatement(),
                _ => ExpressionStatement(),
            };
        }

        private AST.Node FunctionDeclaration()
        {
            Eat(TokenType.Def);
            var name = Identifier();
            Eat(TokenType.OpenParen);

            var parameters = lookAhead?.Type != TokenType.CloseParen
                ? FormalParameterList()
                : new AST.NodeList();

            Eat(TokenType.CloseParen);
            var body = BlockStatement();

            return new AST.FunctionDeclaration(name, parameters, body);
        }

        private AST.NodeList FormalParameterList()
        {
            var parameters = new AST.NodeList();

            do
            {
                parameters.Add(Identifier());
            } while (lookAhead?.Type == TokenType.Comma && (Eat(TokenType.Comma) != null));

            return parameters;
        }

        private AST.Node ReturnStatement()
        {
            Eat(TokenType.Return);

            var argument = lookAhead?.Type != TokenType.Semicolon
                ? Expression()
                : null;

            Eat(TokenType.Semicolon, true);

            return new AST.ReturnStatement(argument);
        }

        private AST.Node IfStatement()
        {
            Eat(TokenType.If);
            Eat(TokenType.OpenParen);
            var test = Expression();
            Eat(TokenType.CloseParen);

            var consequent = Statement();

            var alternate = lookAhead?.Type == TokenType.Else && (Eat(TokenType.Else) != null)
                ? Statement()
                : null;

            return new AST.IfStatement(test, consequent, alternate);
        }

        private AST.Node IterationStatement()
        {
            return (lookAhead?.Type) switch
            {
                TokenType.While => WhileStatement(),
                TokenType.Do => DoWhileStatement(),
                TokenType.Foreach => ForeachStatement(),
                _ => ForStatement(),
            };
        }

        private AST.Node WhileStatement()
        {
            Eat(TokenType.While);
            Eat(TokenType.OpenParen);
            var test = Expression();
            Eat(TokenType.CloseParen);
            var body = Statement();

            return new AST.WhileStatement(test, body);
        }

        private AST.Node DoWhileStatement()
        {
            Eat(TokenType.Do);
            var body = Statement();
            Eat(TokenType.While);
            Eat(TokenType.OpenParen);
            var test = Expression();
            Eat(TokenType.CloseParen);
            Eat(TokenType.Semicolon);

            return new AST.DoWhileStatement(body, test);
        }

        private AST.Node ForeachStatement()
        {
            Eat(TokenType.Foreach);
            Eat(TokenType.OpenParen);
            Eat(TokenType.Let);
            var id = ForeachStatementId();
            Eat(TokenType.In);
            var enumerator = Expression();
            Eat(TokenType.CloseParen);
            var body = Statement();
            return new AST.ForeachStatement(id, enumerator, body);
        }

        private AST.Node ForeachStatementId()
        {
            var id = Eat(TokenType.Identifier);
            return new AST.Identifier(id.Value);
        }

        private AST.Node ForStatement()
        {
            Eat(TokenType.For);
            Eat(TokenType.OpenParen);

            var @init = lookAhead?.Type != TokenType.Semicolon ? ForStatementInit() : null;
            Eat(TokenType.Semicolon);

            var test = lookAhead?.Type != TokenType.Semicolon ? Expression() : null;
            Eat(TokenType.Semicolon);

            var update = lookAhead?.Type != TokenType.CloseParen ? Expression() : null;
            Eat(TokenType.CloseParen);

            var body = Statement();
            return new AST.ForStatement(@init, test, update, body);
        }

        private AST.Node ForStatementInit()
        {
            if (lookAhead?.Type == TokenType.Let)
            {
                return VariableStatementInit();
            }
            return Expression();
        }

        private AST.Node VariableStatementInit()
        {
            Eat(TokenType.Let);
            var declarations = VariableDeclarationsList();
            return new AST.VariableStatement(declarations);
        }


        private AST.Node VariableStatement()
        {
            var variableStatement = VariableStatementInit();
            Eat(TokenType.Semicolon, true);
            return variableStatement;
        }

        private AST.NodeList VariableDeclarationsList()
        {
            var declarations = new AST.NodeList();

            do
            {
                declarations.Add(VariableDeclaration());
            } while (lookAhead?.Type == TokenType.Comma && (Eat(TokenType.Comma) != null));

            return declarations;
        }

        private AST.Node VariableDeclaration()
        {
            var id = Identifier();

            var initialiser = lookAhead?.Type != TokenType.Comma && lookAhead?.Type != TokenType.Semicolon
                    ? VariableInitialiser()
                    : null;

            return new AST.VariableDeclaration(id, initialiser);
        }

        private AST.Node VariableInitialiser()
        {
            Eat(TokenType.SimpleAssign);
            return AssignmentExpression();
        }

        private AST.Node EmptyStatement()
        {
            Eat(TokenType.Semicolon);
            return new AST.EmptyStatement();
        }

        private AST.Node BlockStatement()
        {
            Eat(TokenType.OpenCurly);
            var body = this.lookAhead?.Type != TokenType.CloseCurly ? StatementList(TokenType.CloseCurly) : new AST.NodeList();
            Eat(TokenType.CloseCurly);
            return new AST.BlockStatement(body);
        }

        private AST.Node ExpressionStatement()
        {
            var expression = Expression();
            Eat(TokenType.Semicolon, true);
            return new AST.ExpressionStatement(expression);
        }

        private AST.Node Expression()
        {
            if (lookAhead == null)
            {
                throw CreateSyntaxError(@$"Unexpected end of input: expected an expression.");
            }

            return AssignmentExpression();
        }

        private AST.Node AssignmentExpression()
        {
            var left = this.NullCoalescingExpression();

            if (!(lookAhead?.Type.IsAssignmentOperator() ?? false))
            {
                return left;
            }

            return new AST.AssignmentExpression(
                AssignmentOperator().Value,
                CheckValidAssignmentTarget(left),
                AssignmentExpression());
        }

        private Token AssignmentOperator()
        {
            if (lookAhead?.Type == TokenType.SimpleAssign)
            {
                return Eat(TokenType.SimpleAssign);
            }
            return Eat(TokenType.ComplexAssign);
        }

        private AST.Node LogicalAndExpression()
        {
            return LogicalExpression(TokenType.LogicalAnd, EqualityExpression);
        }

        private AST.Node LogicalOrExpression()
        {
            return LogicalExpression(TokenType.LogicalOr, LogicalAndExpression);
        }

        private AST.Node NullCoalescingExpression()
        {
            return LogicalExpression(TokenType.NullCoalescing, LogicalOrExpression);
        }

        private AST.Node RelationalExpression()
        {
            return BinaryExpression(TokenType.RelationalOperator, AdditiveExpression);
        }

        private AST.Node EqualityExpression()
        {
            return BinaryExpression(TokenType.EqualityOperator, RelationalExpression);
        }

        private AST.Node LeftHandSideExpression()
        {
            return this.CallMemberExpression();
        }

        private AST.Node CallMemberExpression()
        {
            var member = MemberExpression();

            if (lookAhead?.Type == TokenType.OpenParen)
            {
                return CallExpression(member);
            }

            return member;
        }

        private AST.Node CallExpression(AST.Node callee)
        {
            AST.Node callExpression = new AST.CallExpression(callee, Arguments());

            if (lookAhead?.Type == TokenType.OpenParen)
            {
                callExpression = CallExpression(callExpression);
            }

            if (lookAhead?.Type == TokenType.Dot)
            {
                var member = MemberExpressionChain(callExpression);

                if (lookAhead?.Type == TokenType.OpenParen)
                {
                    return CallExpression(member);
                }

                return member;
            }

            return callExpression;
        }

        private AST.NodeList Arguments()
        {
            Eat(TokenType.OpenParen);

            var argumentList = lookAhead?.Type != TokenType.CloseParen
                ? ArgumentList()
                : new AST.NodeList();

            Eat(TokenType.CloseParen);

            return argumentList;
        }

        private AST.NodeList ArgumentList()
        {
            var argumentList = new AST.NodeList();

            do
            {
                argumentList.Add(AssignmentExpression());
            } while (lookAhead?.Type == TokenType.Comma && (Eat(TokenType.Comma) != null));

            return argumentList;
        }

        private AST.Node MemberExpression()
        {
            return MemberExpressionChain(PrimaryExpression());
        }

        private AST.Node MemberExpressionChain(AST.Node @object)
        {
            while (lookAhead?.Type == TokenType.Dot || lookAhead?.Type == TokenType.OpenSquare || lookAhead?.Type == TokenType.NullPropagator)
            {
                if (lookAhead.Type == TokenType.Dot)
                {
                    Eat(TokenType.Dot);
                    var property = Identifier();
                    @object = new AST.MemberExpression(false, false, @object, property);
                }
                else if (lookAhead.Type == TokenType.NullPropagator)
                {
                    Eat(TokenType.NullPropagator);
                    if (lookAhead.Type == TokenType.OpenSquare)
                    {
                        Eat(TokenType.OpenSquare);
                        var property = Expression();
                        Eat(TokenType.CloseSquare);

                        @object = new AST.MemberExpression(true, true, @object, property);
                    }
                    else
                    {
                        var property = Identifier();
                        @object = new AST.MemberExpression(true, false, @object, property);
                    }

                }
                else
                {
                    Eat(TokenType.OpenSquare);
                    var property = Expression();
                    Eat(TokenType.CloseSquare);

                    @object = new AST.MemberExpression(false, true, @object, property);
                }
            }

            return @object;
        }

        private AST.Node Identifier()
        {
            var token = Eat(TokenType.Identifier);
            return new AST.Identifier(token.Value);
        }

        private AST.Node CheckValidAssignmentTarget(AST.Node node)
        {
            if (node.Type == nameof(AST.Identifier) || node.Type == nameof(AST.MemberExpression))
            {
                return node;
            }
            throw CreateSyntaxError("Invalid left-hand side in assignment expression");
        }


        private AST.Node BinaryExpression(TokenType operatorTokenType, Func<AST.Node> builder)
        {
            var left = builder();

            while (lookAhead?.Type == operatorTokenType)
            {
                var @operator = Eat(operatorTokenType).Value;
                var right = builder();

                left = new AST.BinaryExpression(@operator, left, right);
            }

            return left;
        }

        private AST.Node LogicalExpression(TokenType operatorTokenType, Func<AST.Node> builder)
        {
            var left = builder();

            while (lookAhead?.Type == operatorTokenType)
            {
                var @operator = Eat(operatorTokenType).Value;
                var right = builder();

                left = new AST.LogicalExpression(@operator, left, right);
            }

            return left;
        }

        private AST.Node AdditiveExpression()
        {
            return BinaryExpression(TokenType.AdditiveOperator, MultiplicativeExpression);
        }

        private AST.Node MultiplicativeExpression()
        {
            return BinaryExpression(TokenType.MultiplicativeOperator, UnaryExpression);
        }

        private AST.Node UnaryExpression()
        {
            Token? @operator = null;
            switch (lookAhead?.Type)
            {
                case TokenType.AdditiveOperator:
                    @operator = Eat(TokenType.AdditiveOperator);
                    break;
                case TokenType.LogicalNot:
                    @operator = Eat(TokenType.LogicalNot);
                    break;
            }

            if (@operator != null)
            {
                return new AST.UnaryExpression(@operator.Value, UnaryExpression());
            }
            return LeftHandSideExpression();

        }

        private AST.Node ParenthesisedExpression()
        {
            Eat(TokenType.OpenParen);
            var expression = Expression();
            Eat(TokenType.CloseParen);
            return expression;
        }

        private AST.Node PrimaryExpression()
        {
            if (lookAhead == null)
            {
                throw CreateSyntaxError(@$"Unexpected end of input: expected an expression.");
            }

            if (lookAhead?.Type.IsLiteral() ?? false)
            {
                return Literal();
            }

            return (lookAhead?.Type) switch
            {
                TokenType.OpenTemplateLiteral => TemplateLiteral(),
                TokenType.OpenParen => ParenthesisedExpression(),
                TokenType.Identifier => Identifier(),
                _ => throw CreateSyntaxError(@$"Invalid expression ""{lookAhead?.Value}"""),
            };
        }

        private AST.Node TemplateLiteral()
        {
            Eat(TokenType.OpenTemplateLiteral);

            var segments = new AST.NodeList();

            while (lookAhead?.Type != TokenType.CloseTemplateLiteral)
            {
                switch (lookAhead?.Type)
                {
                    case TokenType.PartialString:
                        segments.Add(PartialString());
                        break;

                    case TokenType.OpenCurly:
                        segments.Add(TemplateLiteralExpression());
                        break;
                }
            }

            Eat(TokenType.CloseTemplateLiteral);

            return new AST.TemplateLiteral(segments);
        }

        private AST.Node TemplateLiteralExpression()
        {
            Eat(TokenType.OpenCurly);
            var expression = Expression();
            Eat(TokenType.CloseCurly);
            return expression;
        }

        private AST.Node NumericLiteral()
        {
            var token = this.Eat(TokenType.Number);
            if (token.Value.StartsWith("£"))
            {
                return new AST.NumericLiteral(token.Value, decimal.Parse(token.Value[1..]));
            }
            else if (token.Value.EndsWith("%"))
            {
                var percentage = decimal.Parse(token.Value[0..^1]);
                return new AST.NumericLiteral(token.Value, percentage / 100m);
            }
            else
            {
                return new AST.NumericLiteral(token.Value, decimal.Parse(token.Value));
            }
        }

        private AST.Node PartialString()
        {
            var token = this.Eat(TokenType.PartialString);
            return new AST.PartialString(token.Value);
        }

        private AST.Node StringLiteral()
        {
            var token = this.Eat(TokenType.String);
            return new AST.StringLiteral(token.Value[1..^1]);
        }

        private AST.Node BooleanLiteral(TokenType tokenType)
        {
            var token = this.Eat(tokenType);
            return new AST.BooleanLiteral(bool.Parse(token.Value));
        }

        private AST.Node NullLiteral()
        {
            this.Eat(TokenType.Null);
            return new AST.NullLiteral();
        }

        private Token Eat(TokenType tokenType, bool ignoreEndOfInput = false)
        {
            var token = this.lookAhead;

            if (token == null)
            {
                if (ignoreEndOfInput)
                {
                    return tokeniser.GetImplicitToken(tokenType, string.Empty);
                }

                throw CreateSyntaxError($"Unexpected end of input, expected: {Enum.GetName(tokenType)}");
            }

            if (token.Type != tokenType)
            {
                throw CreateSyntaxError(@$"Unexpected token ""{token.Value}"", expected: {Enum.GetName(tokenType)}");
            }

            this.lookAhead = tokeniser.GetNextToken();

            return token;
        }

        private SyntaxErrorException CreateSyntaxError(string message)
        {
            return new SyntaxErrorException($"{message} at position {lookAhead?.Start}");
        }
    }
}
