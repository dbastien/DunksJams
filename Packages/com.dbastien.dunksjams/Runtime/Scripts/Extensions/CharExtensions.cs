public static class CharExtensions
{
    public static bool IsLower(this char c) => char.IsLower(c);
    public static bool IsUpper(this char c) => char.IsUpper(c);
    public static bool IsDigit(this char c) => char.IsDigit(c);
    public static bool IsLetter(this char c) => char.IsLetter(c);
    public static bool IsWhitespace(this char c) => char.IsWhiteSpace(c);
    public static char ToLower(this char c) => char.ToLower(c);
    public static char ToUpper(this char c) => char.ToUpper(c);
}