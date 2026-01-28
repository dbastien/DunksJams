using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    //precompiled regexes
    public const RegexOptions Compiled = RegexOptions.Compiled;
    public static readonly Lazy<Regex> EmailRegex = new(() => new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", Compiled));
    public static readonly Lazy<Regex> Base64Regex = new(() => new(@"^[a-zA-Z0-9\+/]*={0,2}$", Compiled));
    public static readonly Lazy<Regex> Ipv4Regex = new(() => new(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", Compiled));
    public static readonly Lazy<Regex> Ipv6Regex = new(() => new(@"^(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$", Compiled));
    public static readonly Lazy<Regex> MacAddressRegex = new(() => new(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$", Compiled));
    public static readonly Lazy<Regex> HexColorRegex = new(() => new(@"^#?([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$", Compiled));
    public static readonly Lazy<Regex> HexRegex = new(() => new(@"^0x[0-9A-Fa-f]+$", Compiled));
    public static readonly Lazy<Regex> NumberRegex = new(() => new(@"^-?[0-9]+(?:\.[0-9]+)?$", Compiled));
    public static readonly Lazy<Regex> IntegerRegex = new(() => new(@"^-?[0-9]+$", Compiled));
    public static readonly Lazy<Regex> DecimalRegex = new(() => new(@"^-?[0-9]+\.[0-9]+$", Compiled));
    public static readonly Lazy<Regex> DateRegex = new(() => new(@"^\d{4}-\d{2}-\d{2}$", Compiled));
    public static readonly Lazy<Regex> TimeRegex = new(() => new(@"^\d{2}:\d{2}:\d{2}$", Compiled));
    public static readonly Lazy<Regex> DateTimeRegex = new(() => new(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$", Compiled));
    public static readonly Lazy<Regex> URLRegex = new(() => new(@"^(https?|ftp)://[^\s/$.?#].[^\s]*$", Compiled | RegexOptions.IgnoreCase));
    public static readonly Lazy<Regex> GuidRegex = new(() => new(@"^[{(]?[0-9a-fA-F]{8}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{4}[-]?[0-9a-fA-F]{12}[)}]?$", Compiled));
    public static readonly Lazy<Regex> CamelCaseSplitRegex = new(() => new(@"([a-z])([A-Z])", Compiled));
    public static readonly Lazy<Regex> CamelCaseRegex = new(() => new(@"(\P{Ll})(\p{Ll})", Compiled));
    public static readonly Lazy<Regex> HtmlTagRegex = new(() => new(@"<[^>]*>", Compiled));
    public static readonly Lazy<Regex> HtmlEntityRegex = new(() => new(@"&[^;]+;", Compiled));
    public static readonly Lazy<Regex> HtmlCommentRegex = new(() => new(@"<!--.*?-->", Compiled));

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

    public static bool IsAllDigits(this string s)
    {
        foreach (char c in s) if (!char.IsDigit(c)) return false;
        return true;
    }

    public static bool IsAllLetters(this string s)
    {
        foreach (char c in s) if (!char.IsLetter(c)) return false;
        return true;
    }

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

    public static float[] ToNFloats(this string s, int n)
    {
        string[] parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != n) return null;

        var result = new float[n];
        for (int i = 0; i < n; i++)
            if (!float.TryParse(parts[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result[i])) return null;
        return result;
    }
    
    public static string SplitCamelCase(this string s) => CamelCaseRegex.Value.Replace(s, "$1 $2");
    public static string Slugify(this string s) => Regex.Replace(s.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');

    public static string ToTitleCase(this string s) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s);
    public static string ToCamelCase(this string s) => s.ToTitleCase().Replace(" ", "");

    public static string ToSnakeCase(this string s)
    {
        var sb = new StringBuilder(s.Length + 5);
        for (int i = 0; i < s.Length; ++i)
        {
            if (i > 0 && char.IsUpper(s[i]) && !char.IsWhiteSpace(s[i - 1])) sb.Append('_');
            sb.Append(char.ToLowerInvariant(s[i]));
        }
        return sb.ToString();
    }

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
            if (slice.Length > 0 && slice[^1] == '\r')
                slice = slice[..^1];
            lines.Add(slice.ToString());
            start = i + 1;
        }

        if (start < data.Length)
            lines.Add(data[start..].Trim().ToString());

        return lines;
    }
}