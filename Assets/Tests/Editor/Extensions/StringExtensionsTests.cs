using NUnit.Framework;

public class StringExtensionsTests : TestBase
{
    [Test] public void RemovePrefix()
    {
        Eq("World", "HelloWorld".RemovePrefix("Hello"));
        Eq("HelloWorld", "HelloWorld".RemovePrefix("Bye"));
        Eq("", "Hello".RemovePrefix("Hello"));
    }

    [Test] public void RemoveSuffix()
    {
        Eq("Hello", "HelloWorld".RemoveSuffix("World"));
        Eq("HelloWorld", "HelloWorld".RemoveSuffix("Bye"));
        Eq("", "Hello".RemoveSuffix("Hello"));
    }

    [Test] public void Remove()
    {
        Eq("Hllo", "Hello".Remove("e"));
        Eq("Hello", "Hello".Remove("x"));
    }

    [Test] public void RemoveWhiteSpace()
    {
        Eq("abc", " a b c ".RemoveWhiteSpace());
        Eq("abc", "abc".RemoveWhiteSpace());
    }

    [Test] public void IsAllDigits()
    {
        True("123".IsAllDigits());
        True("0".IsAllDigits());
        False("-123".IsAllDigits());  // hyphen is not a digit
        False("12.34".IsAllDigits()); // dot is not a digit
        False("abc".IsAllDigits());
    }

    [Test] public void CsvSafe()
    {
        Eq("a b c", "a,b,c".CsvSafe());
    }
}
