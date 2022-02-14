namespace PotiScript.Grammar
{
    public record Token(TokenType Type, string Value, int Start, int End);
}
