namespace PotiScript.Grammar
{
    public enum TokenType
    {
        // Symbols, delimiters
        Semicolon,
        OpenCurly,
        CloseCurly,
        OpenParen,
        CloseParen,
        Comma,
        Dot,
        OpenSquare,
        CloseSquare,
        NullPropagator,

        // keywords
        Let,
        If,
        Else,
        True,
        False,
        Null,
        While,
        Do,
        For,
        Foreach,
        In,
        Def,
        Return,

        // Literals
        Number,
        String,

        // string interpolation
        OpenTemplateLiteral,
        CloseTemplateLiteral,
        PartialString,

        // Operators
        AdditiveOperator,
        MultiplicativeOperator,

        // Relational, Equality
        RelationalOperator,
        EqualityOperator,

        // Logical Operators
        LogicalAnd,
        LogicalOr,
        LogicalNot,
        NullCoalescing,

        // Assignment
        SimpleAssign,
        ComplexAssign,

        // Identifiers
        Identifier,
        Var,
        Func,
    }

    public static class TokenTypeExtensions
    {
        public static bool IsLiteral(this TokenType tokenType)
        {
            return tokenType == TokenType.Number
                || tokenType == TokenType.String
                || tokenType == TokenType.True
                || tokenType == TokenType.False
                || tokenType == TokenType.Null;
        }

        public static bool IsAssignmentOperator(this TokenType tokenType)
        {
            return tokenType == TokenType.SimpleAssign || tokenType == TokenType.ComplexAssign;
        }
    }
}
