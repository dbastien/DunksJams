using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Utilities;

/// <summary>
/// General purpose helper functions for the Dunks Dialog system.
/// Provides string parsing, tag stripping, and field access utilities.
/// </summary>
public static class DialogUtility
{
    private static readonly Regex _dialogTagRegex = new(@"\[.+?\]", RegexOptions.Compiled);
    private static readonly Regex _tmproTagRegex = new(@"<.+?>", RegexOptions.Compiled);

    /// Strips both TextMeshPro and Dialog system tags from text.
    public static string StripAllTags(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        string result = _tmproTagRegex.Replace(text, string.Empty);
        result = _dialogTagRegex.Replace(result, string.Empty);
        return result;
    }

    public static string ParsePauseTags(string input, out Dictionary<int, float> pauses)
    {
        pauses = new Dictionary<int, float>();
        if (string.IsNullOrEmpty(input)) return string.Empty;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        int visibleIndex = 0;
        for (int i = 0; i < input.Length; i++)
        {
            // Check for TMPro tag
            if (input[i] == '<')
            {
                int end = input.IndexOf('>', i);
                if (end != -1)
                {
                    sb.Append(input.Substring(i, end - i + 1));
                    i = end;
                    continue;
                }
            }

            // Check for Pause tag [p=0.5]
            if (input[i] == '[')
            {
                var sub = input.Substring(i);
                var match = Regex.Match(sub, @"^\[p(?:ause)?=([\d\.]+)\]");
                if (match.Success)
                {
                    if (float.TryParse(match.Groups[1].Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float duration)) pauses[visibleIndex] = duration;
                    i += match.Length - 1;
                    continue;
                }
            }

            sb.Append(input[i]);
            visibleIndex++;
        }

        return sb.ToString();
    }

    /// Processes a string by replacing [var=Name] tags with values from a lookup function.
    public static string ProcessText(string input, Func<string, string> getVariable)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return Regex.Replace(input, @"\[var=(.+?)\]", m => getVariable?.Invoke(m.Groups[1].Value) ?? m.Value);
    }

    /// Evaluates a simple condition (e.g. "VarName==Value").
    public static bool EvaluateCondition(string condition, Func<string, string> getVariable)
    {
        if (string.IsNullOrEmpty(condition)) return true;

        if (condition.Contains("=="))
        {
            var parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
            if (parts.Length == 2) return (getVariable?.Invoke(parts[0].Trim()) ?? string.Empty) == parts[1].Trim();
        }
        // Add more operators here later (>=, <=, !=, etc.)
        return true;
    }

    /// Executes a simple script (e.g. "VarName=Value").
    public static void ExecuteScript(string script, Action<string, string> setVariable)
    {
        if (string.IsNullOrEmpty(script)) return;

        if (script.Contains("="))
        {
            var parts = script.Split('=');
            if (parts.Length == 2)
            {
                string left = parts[0].Trim();
                string right = parts[1].Trim();

                // Handle Quest["Name"].State = "Value"
                var questMatch = Regex.Match(left, @"Quest\[""(.+?)""\]\.State");
                if (questMatch.Success)
                {
                    string questName = questMatch.Groups[1].Value;
                    if (QuestManager.Instance != null) QuestManager.Instance.SetQuestState(questName, right);
                }
                else
                {
                    setVariable?.Invoke(left, right);
                }
            }
        }
    }

    /// <summary>
    /// Scans all assemblies for types that match a specific name prefix.
    /// Useful for discovering sequencer commands (e.g. prefix "SequencerCommand").
    /// </summary>
    public static Dictionary<string, Type> DiscoverTypesWithPrefix(string prefix)
    {
        var result = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        var types = ReflectionUtils.GetTypesWithPrefix(prefix);
        foreach (var type in types)
        {
            var key = type.Name.Substring(prefix.Length);
            result[key] = type;
        }
        return result;
    }

    /// <summary>
    /// Robust argument parser for sequencer commands that handles quotes and commas within quotes.
    /// e.g. 'Arg1, "Arg 2, with comma", Arg3' -> ["Arg1", "Arg 2, with comma", "Arg3"]
    /// </summary>
    public static string[] ParseCommandArguments(string argsString)
    {
        if (string.IsNullOrEmpty(argsString)) return Array.Empty<string>();

        List<string> result = new List<string>();
        bool inQuotes = false;
        int start = 0;

        for (int i = 0; i < argsString.Length; i++)
            if (argsString[i] == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (argsString[i] == ',' && !inQuotes)
            {
                result.Add(argsString.Substring(start, i - start).Trim().Trim('\"'));
                start = i + 1;
            }

        result.Add(argsString.Substring(start).Trim().Trim('\"'));
        return result.ToArray();
    }

    /// <summary>
    /// Finds a Transform in the scene by name or tag.
    /// Returns speaker/listener fallback if name matches special keywords.
    /// </summary>
    public static Transform FindTransform(string name, Transform speaker = null, Transform listener = null)
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (name.Equals("speaker", StringComparison.OrdinalIgnoreCase)) return speaker;
        if (name.Equals("listener", StringComparison.OrdinalIgnoreCase)) return listener;

        GameObject go = GameObjectUtils.Find(name);
        return go != null ? go.transform : null;
    }

    /// Checks if a list of fields contains a specific field by name.
    public static bool HasField(this List<Field> fields, string name)
    {
        return fields != null && fields.Exists(f => f.name == name);
    }

    /// Gets a field value string or fallback.
    public static string GetFieldValue(this List<Field> fields, string name, string fallback = "")
    {
        var field = fields?.Find(f => f.name == name);
        return field != null ? field.value : fallback;
    }

    /// Gets a field value cast to the desired type, or fallback if not found/invalid.
    public static T GetField<T>(this List<Field> fields, string name, T fallback = default)
    {
        var field = fields?.Find(f => f.name == name);
        if (field == null) return fallback;

        if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) return field.objValue is T val ? val : fallback;

        // Using ReflectionUtils to convert the string value
        return (T)ReflectionUtils.ConvertToType(field.value, typeof(T));
    }

    /// Sets or adds a field in the list.
    public static void SetField(this List<Field> fields, string name, string value, FieldType type = FieldType.Text)
    {
        if (fields == null) return;
        var field = fields.Find(f => f.name == name);
        if (field != null)
        {
            field.value = value;
            field.type = type;
        }
        else
        {
            fields.Add(new Field(name, value, type));
        }
    }

    /// Sets or adds an object field in the list.
    public static void SetField(this List<Field> fields, string name, UnityEngine.Object objValue)
    {
        if (fields == null) return;
        var field = fields.Find(f => f.name == name);
        if (field != null)
        {
            field.objValue = objValue;
            field.type = FieldType.Object;
        }
        else
        {
            var newField = new Field { name = name, objValue = objValue, type = FieldType.Object };
            fields.Add(newField);
        }
    }
}