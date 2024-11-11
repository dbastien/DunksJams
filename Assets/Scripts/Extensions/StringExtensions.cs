using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class StringExtensions
{
    public static string Join(this IEnumerable<string> s, char c) => string.Join(c, s); 
    public static string Join(this IEnumerable<object> s, char c) => string.Join(c, s);
    public static string Remove(this string s, string r) => s.Replace(r, "");
    public static string RemovePrefix(this string s, string prefix) =>
        s.StartsWithFast(prefix) ? s[prefix.Length..] : s;
    public static string RemoveSuffix(this string s, string suffix) =>
        s.EndsWithFast(suffix) ? s[..^suffix.Length] : s;
    
    public static string RemoveWhiteSpace(this string s)
    {
        var result = new StringBuilder(s.Length);
        foreach (char c in s)
            if (!char.IsWhiteSpace(c)) 
                result.Append(c);
        return result.ToString();
    }
    
    public static string CsvSafe(this string s) => s.Replace(',', ' ');
    public static bool IsAllDigits(this string s) => s.All(char.IsDigit);
    public static bool IsAllLetters(this string s) => s.All(char.IsLetter);
    
    public static bool IsPalindrome(this string s)
    {
        for (int i = 0, j = s.Length - 1; i < j; ++i, --j)
            if (char.ToLowerInvariant(s[i]) != char.ToLowerInvariant(s[j]))
                return false;
        return true;
    }
    
    public static string GetPlural(this string s, int count, string pluralForm = null) => 
        count == 1 || s == pluralForm ? s : pluralForm ?? (s.EndsWith("s") ? $"{s}es" : $"{s}s");

    public static string Reverse(this string s)
    {
        var len = s.Length;
        Span<char> buffer = stackalloc char[len];
        for (int i = 0, j = len - 1; i < len; ++i, --j)
            buffer[i] = s[j];
        return new string(buffer);
    }
    
    //improved from https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity5.html
    //line, substring
    public static bool StartsWithFast(this string a, string b)
    {
        int aLen = a.Length;
        int bLen = b.Length;
        int p = 0;

        if (aLen < bLen) return false;
        while (p < bLen && a[p] == b[p]) ++p;
        return p == bLen;
    }

    public static bool EndsWithFast(this string a, string b)
    {
        int ap = a.Length - 1;
        int bp = b.Length - 1;

        while (ap >= 0 && bp >= 0 && a[ap] == b[bp]) { --ap; --bp; }

        return (bp < 0 && a.Length >= b.Length) || 
               (ap < 0 && b.Length >= a.Length);
    }
    
    public static float[] To3Floats(this string s)
    {
        string[] parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return null;

        if (float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y) &&
            float.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z))
        {
            return new[] { x, y, z };
        }

        return null;
    }
    
    public static string SplitCamelCase(this string s) =>
        System.Text.RegularExpressions.Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");

    public static string Truncate(this string s, int len) => s.Length <= len ? s : s[..len];
    
    //todo: maybe move this somewhere else
    public static List<string> SplitLines(this ReadOnlySpan<char> data)
    {
        var lines = new List<string>();
        var start = 0;

        for (var i = 0; i < data.Length; ++i)
        {
            if (data[i] != '\n') continue;

            var slice = data[start..i];
            if (slice.Length > 0 && slice[^1] == '\r') // Handle \r\n
                slice = slice[..^1];
            lines.Add(slice.ToString());
            start = i + 1;
        }

        if (start < data.Length) // Handle final line without newline
            lines.Add(data[start..].Trim().ToString());

        return lines;
    }
}