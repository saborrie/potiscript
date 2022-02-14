using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PotiScript.Exceptions;

namespace PotiScript.Grammar
{
    public class Tokeniser
    {
        private readonly (string, TokenType?)[] spec = GetSpec().ToArray();
        private readonly (string, TokenType?)[] specTemplateLiteral = GetSpecForTemplateLiteral().ToArray();
        private string text = string.Empty;
        private int cursor;
        private Stack<TokeniserState> stack = new();

        private enum TokeniserState
        {
            Normal,
            InsideTemplateLiteral
        }

        public void Init(string text)
        {
            this.text = text;
            this.cursor = 0;
            this.stack = new Stack<TokeniserState>();
            stack.Push(TokeniserState.Normal);
        }

        public Token GetImplicitToken(TokenType tokenType, string value)
        {
            return new Token(tokenType, value, cursor, cursor);
        }

        public Token? GetNextToken()
        {
            if (!this.HasMoreTokens())
            {
                return null;
            }

            var subtext = text[cursor..];

            var currentSpec = stack.Peek() switch
            {
                TokeniserState.Normal => spec,
                TokeniserState.InsideTemplateLiteral => specTemplateLiteral,
                _ => throw new Exception("Tokeniser entered invalid mode. This should not happen.")
            };

            foreach (var (pattern, type) in currentSpec)
            {
                var match = Regex.Match(subtext, pattern);

                if (match.Success)
                {
                    cursor += match.Length;

                    if (type.HasValue)
                    {
                        if (type == TokenType.OpenCurly)
                        {
                            stack.Push(TokeniserState.Normal);
                        }
                        else if (type == TokenType.CloseCurly)
                        {
                            stack.Pop();
                        }
                        else if (type == TokenType.OpenTemplateLiteral)
                        {
                            stack.Push(TokeniserState.InsideTemplateLiteral);
                        }
                        else if (type == TokenType.CloseTemplateLiteral)
                        {
                            stack.Pop();
                        }

                        return new Token(type.Value, match.Value, cursor - match.Length, cursor);
                    }
                    else
                    {
                        return GetNextToken();
                    }
                }
            }

            throw new SyntaxErrorException(@$"Unexpected token: ""{subtext[0]}"" at position {cursor}");
        }

        public bool HasMoreTokens()
        {
            return cursor < text.Length;
        }

        private static IEnumerable<(string, TokenType?)> GetSpecForTemplateLiteral()
        {
            yield return (@"^\{", TokenType.OpenCurly);
            yield return (@"^""", TokenType.CloseTemplateLiteral);
            yield return (@"^[^""^\{]+", TokenType.PartialString);
        }

        private static IEnumerable<(string, TokenType?)> GetSpec()
        {
            // Whitespace
            yield return (@"^\s+", null);

            // Comments
            yield return (@"^/\*[\s\S]*?\*/", null);

            // Symbols, delimiters
            yield return (@"^;", TokenType.Semicolon);
            yield return (@"^\{", TokenType.OpenCurly);
            yield return (@"^\}", TokenType.CloseCurly);
            yield return (@"^\(", TokenType.OpenParen);
            yield return (@"^\)", TokenType.CloseParen);
            yield return (@"^\,", TokenType.Comma);
            yield return (@"^\?\.", TokenType.NullPropagator);
            yield return (@"^\.", TokenType.Dot);
            yield return (@"^\[", TokenType.OpenSquare);
            yield return (@"^\]", TokenType.CloseSquare);
            yield return (@"^\$\""", TokenType.OpenTemplateLiteral);

            // Keywords
            yield return (@"^\blet\b", TokenType.Let);
            yield return (@"^\bif\b", TokenType.If);
            yield return (@"^\belse\b", TokenType.Else);
            yield return (@"^\btrue\b", TokenType.True);
            yield return (@"^\bfalse\b", TokenType.False);
            yield return (@"^\bnull\b", TokenType.Null);
            yield return (@"^\bwhile\b", TokenType.While);
            yield return (@"^\bdo\b", TokenType.Do);
            yield return (@"^\bfor\b", TokenType.For);
            yield return (@"^\bforeach\b", TokenType.Foreach);
            yield return (@"^\bin\b", TokenType.In);
            yield return (@"^\bdef\b", TokenType.Def);
            yield return (@"^\breturn\b", TokenType.Return);

            // Literals
            yield return (@"^\d+(\.\d+)?\%", TokenType.Number);
            yield return (@"^\d+(\.\d+)?", TokenType.Number);
            yield return (@"^""[^""]*""", TokenType.String);
            yield return (@"^'[^']*'", TokenType.String);

            // Relational, Equality
            yield return (@"^[><]=?", TokenType.RelationalOperator);
            yield return (@"^[!=]=", TokenType.EqualityOperator);

            // Logical
            yield return (@"^\?\?", TokenType.NullCoalescing);
            yield return (@"^&&", TokenType.LogicalAnd);
            yield return (@"^\|\|", TokenType.LogicalOr);
            yield return (@"^!", TokenType.LogicalNot);

            // Assignment
            yield return (@"^=", TokenType.SimpleAssign);
            yield return (@"^[\*\/\+\-]=", TokenType.ComplexAssign);

            // Operators
            yield return (@"^[+\-]", TokenType.AdditiveOperator);
            yield return (@"^[*\/]", TokenType.MultiplicativeOperator);

            // Identifiers
            yield return (@"^\w+", TokenType.Identifier);
        }
    }
}
