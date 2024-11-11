using System;
using System.Collections.Generic;
using System.Text;

public static class EnumCache<T> where T : Enum
{
    public static readonly T[] Values;
    static readonly Dictionary<T, string> Cache;
    static readonly string[] Strings;
    static readonly string _summary;
    
    static EnumCache()
    {
        Values = (T[])Enum.GetValues(typeof(T));
        Strings = new string[Values.Length];
        Cache = new(Values.Length);

        StringBuilder sb = new();
        for (int i = 0; i < Values.Length; ++i)
        {
            var val = Values[i];
            var valString = Strings[i];
            Cache[val] = valString;
            sb.AppendLine($"{val}: \"{valString}\"");
        }

        _summary = sb.ToString();
    }

    public static string GetName(T value) => Cache[value];
    public static string GetSummary() => _summary;
}