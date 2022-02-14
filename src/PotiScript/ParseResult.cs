namespace PotiScript
{
    public class ParseResult
    {
        public bool IsSuccess { get; private set; }

        public string? Error { get; private set; }

        public static ParseResult CreateSuccess() => new ParseResult { IsSuccess = true };
        public static ParseResult CreateError(string error) => new ParseResult { IsSuccess = false, Error = error };
    }
}
