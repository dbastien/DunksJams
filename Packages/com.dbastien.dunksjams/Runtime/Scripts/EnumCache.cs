using System;
using System.Collections.Generic;
using System.Text;

public static class EnumCache<T> where T : Enum
{
    public static readonly T[] Values;
    private static readonly Dictionary<T, string> Cache;
    private static readonly string[] Strings;
    private static readonly string _summary;

    static EnumCache()
    {
        Values = (T[])Enum.GetValues(typeof(T));
        Strings = new string[Values.Length];
        Cache = new Dictionary<T, string>(Values.Length);

        StringBuilder sb = new();
        for (var i = 0; i < Values.Length; ++i)
        {
            T val = Values[i];
            var valString = val.ToString();
            Strings[i] = valString;
            Cache[val] = valString;
            sb.AppendLine($"{val}: \"{valString}\"");
        }

        _summary = sb.ToString();
    }

    public static string GetName(T value) => Cache[value];
    public static string GetSummary() => _summary;
}