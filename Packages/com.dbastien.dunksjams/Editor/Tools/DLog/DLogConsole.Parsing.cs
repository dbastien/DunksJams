// Assets/Editor/DLog/DLogConsole.Parsing.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed partial class DLogConsole
{
    static readonly Regex s_stackAtFileLine = new(@"\(at\s+(.+?):(\d+)\)", RegexOptions.Compiled);
    static readonly Regex s_stackInLine = new(@"\s+in\s+(.+):line\s+(\d+)\s*$", RegexOptions.Compiled);
    static readonly Regex s_stackLooseFileLine = new(@"(.+?\.\w+):(\d+)", RegexOptions.Compiled);
    static readonly Regex s_bracketFileLine = new(@"\[(.+?):(\d+)\]", RegexOptions.Compiled);
    static readonly Regex s_richTextTags = new(@"</?(color|a|b)(\s+[^>]*)?>", RegexOptions.Compiled);

    static string s_projectRoot;

    static readonly StackColors s_stackColorsPro = new()
    {
        Namespace = "#9BB3CE",
        Class = "#5CB5FF",
        Method = "#86D6FF",
        Args = "#B6CCE0"
    };

    static readonly StackColors s_stackColorsLight = new()
    {
        Namespace = "#5E6C86",
        Class = "#2B7EB6",
        Method = "#4A7CB0",
        Args = "#7A889E"
    };

    struct StackColors
    {
        public string Namespace;
        public string Class;
        public string Method;
        public string Args;
    }

    void EnsureMessageCache(LogEntry e)
    {
        if (e == null || e.messageLinkParsed)
            return;

        var raw = e.richMessage ?? e.message ?? "";
        e.plainMessage = StripRichText(raw);
        e.messageLinkParsed = true;
        e.messageLink = default;

        if (string.IsNullOrEmpty(e.plainMessage))
            return;

        if (TryGetMessageLinkSpan(e.plainMessage, out var file, out var lineNumber, out var start, out var length))
            e.messageLink = new LinkSpan
            {
                hasLink = true,
                file = file,
                lineNumber = lineNumber,
                start = start,
                length = length
            };
    }

    void EnsureStackCache(LogEntry e)
    {
        if (e == null)
            return;

        var isProSkin = EditorGUIUtility.isProSkin;
        if (e.stackCacheValid && e.stackCacheProSkin == isProSkin)
            return;

        e.stackCacheValid = true;
        e.stackCacheProSkin = isProSkin;
        e.stackLineInfos = null;
        e.stackScrollHeight = 0f;

        if (string.IsNullOrEmpty(e.stackTrace))
            return;

        var lines = e.stackTrace.Split('\n');
        var infos = new List<StackLineInfo>(lines.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var info = new StackLineInfo { line = line };
            if (TryGetStackLinkSpan(line, out var file, out var lineNumber, out var start, out var length))
                info.link = new LinkSpan
                {
                    hasLink = true,
                    file = file,
                    lineNumber = lineNumber,
                    start = start,
                    length = length
                };

            if (TryBuildStackDisplayLine(line, info.link, isProSkin, out var displayLine))
                info.displayLine = displayLine;
            else if (info.link.hasLink)
                info.displayLine = WrapHighlight(line, info.link.start, info.link.length);
            else
                info.displayLine = line;

            infos.Add(info);
        }

        e.stackLineInfos = infos.ToArray();
        if (e.stackLineInfos.Length == 0)
            return;

        var lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        var linesToShow = Mathf.Min(MaxStackLinesVisible, e.stackLineInfos.Length);
        e.stackScrollHeight = Mathf.Min(MaxStackHeight, lineHeight * linesToShow);
    }

    static bool TryParseStackLine(string line, out string file, out int lineNumber)
    {
        file = null;
        lineNumber = 0;

        var m = s_stackAtFileLine.Match(line);
        if (m.Success)
        {
            file = m.Groups[1].Value;
            int.TryParse(m.Groups[2].Value, out lineNumber);
            return lineNumber > 0;
        }

        m = s_stackInLine.Match(line);
        if (m.Success)
        {
            file = m.Groups[1].Value;
            int.TryParse(m.Groups[2].Value, out lineNumber);
            return lineNumber > 0;
        }

        m = s_stackLooseFileLine.Match(line);
        if (m.Success)
        {
            file = m.Groups[1].Value;
            int.TryParse(m.Groups[2].Value, out lineNumber);
            return lineNumber > 0;
        }

        return false;
    }

    static string ToAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var p = path.Replace("\\", "/");
        var idx = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? p.Substring(idx) : p;
    }

    static string ProjectRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(s_projectRoot)) return s_projectRoot;

            var dataPath = Application.dataPath.Replace("\\", "/");
            if (dataPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
                s_projectRoot = dataPath.Substring(0, dataPath.Length - "/Assets".Length);
            else
                s_projectRoot = Path.GetDirectoryName(dataPath) ?? dataPath;

            return s_projectRoot;
        }
    }

    static string ToFullPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        var p = path.Replace("\\", "/");
        if (Path.IsPathRooted(p))
            return p;

        var root = ProjectRoot;
        if (string.IsNullOrEmpty(root))
            return p;

        return Path.Combine(root, p).Replace("\\", "/");
    }

    bool TryGetMessageLinkSpan(string line, out string file, out int lineNumber, out int start, out int length)
    {
        if (TryGetStackLinkSpan(line, out file, out lineNumber, out start, out length))
            return true;

        var match = s_bracketFileLine.Match(line);
        return TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length);
    }

    bool TryGetStackLinkSpan(string line, out string file, out int lineNumber, out int start, out int length)
    {
        file = null;
        lineNumber = 0;
        start = 0;
        length = 0;

        if (string.IsNullOrEmpty(line))
            return false;

        var match = s_stackAtFileLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackInLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackLooseFileLine.Match(line);
        return TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length);
    }

    bool TryBuildLinkSpan(Match match, string line, out string file, out int lineNumber, out int start,
        out int length)
    {
        file = null;
        lineNumber = 0;
        start = 0;
        length = 0;

        if (match == null || !match.Success || match.Groups.Count < 3)
            return false;

        file = match.Groups[1].Value;
        if (file.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return false;
        if (!int.TryParse(match.Groups[2].Value, out lineNumber) || lineNumber <= 0)
            return false;

        string fileName;
        try
        {
            fileName = Path.GetFileName(file);
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (string.IsNullOrEmpty(fileName))
            return false;

        var pathStart = match.Groups[1].Index;
        var offset = file.Replace("\\", "/").LastIndexOf('/');
        if (offset >= 0)
        {
            offset += 1;
        }
        else
        {
            offset = file.LastIndexOf('\\');
            offset = offset >= 0 ? offset + 1 : 0;
        }

        start = pathStart + offset;
        var token = $"{fileName}:{lineNumber}";
        length = token.Length;

        // If the line uses ":line N", expand to include it for readability.
        var lineTokenIndex = line.IndexOf($"{fileName}:line {lineNumber}", StringComparison.OrdinalIgnoreCase);
        if (lineTokenIndex >= 0)
        {
            start = lineTokenIndex;
            length = $"{fileName}:line {lineNumber}".Length;
        }

        if (start < 0 || start + length > line.Length)
            return false;

        return true;
    }

    static bool TryGetFileName(string path, out string fileName)
    {
        fileName = null;
        if (string.IsNullOrEmpty(path))
            return false;
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return false;

        try
        {
            fileName = Path.GetFileName(path);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return !string.IsNullOrEmpty(fileName);
    }

    static string StripRichText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return s_richTextTags.Replace(text, "");
    }

    static bool TryBuildStackDisplayLine(string line, LinkSpan link, bool isProSkin, out string displayLine)
    {
        displayLine = null;
        if (string.IsNullOrEmpty(line))
            return false;

        var methodLastIndex = line.IndexOf('(');
        if (methodLastIndex <= 0)
            return false;

        var argsLastIndex = line.IndexOf(')', methodLastIndex);
        if (argsLastIndex <= 0)
            return false;

        var methodFirstIndex = line.LastIndexOf(':', methodLastIndex);
        if (methodFirstIndex <= 0)
            methodFirstIndex = line.LastIndexOf('.', methodLastIndex);
        if (methodFirstIndex <= 0)
            return false;

        var methodString = line.Substring(methodFirstIndex + 1, methodLastIndex - methodFirstIndex - 1);

        string classString;
        var namespaceString = string.Empty;
        var classFirstIndex = line.LastIndexOf('.', methodFirstIndex - 1);
        if (classFirstIndex <= 0)
        {
            classString = line.Substring(0, methodFirstIndex + 1);
        }
        else
        {
            classString = line.Substring(classFirstIndex + 1, methodFirstIndex - classFirstIndex);
            namespaceString = line.Substring(0, classFirstIndex + 1);
        }

        var argsString = line.Substring(methodLastIndex, argsLastIndex - methodLastIndex + 1);
        var suffixStart = argsLastIndex + 1;
        var suffix = suffixStart < line.Length ? line.Substring(suffixStart) : string.Empty;

        if (link.hasLink && suffix.Length > 0)
        {
            var linkStartInSuffix = link.start - suffixStart;
            if (linkStartInSuffix >= 0 && linkStartInSuffix + link.length <= suffix.Length)
                suffix = WrapHighlight(suffix, linkStartInSuffix, link.length);
        }

        var colors = isProSkin ? s_stackColorsPro : s_stackColorsLight;
        displayLine = $"{WrapColor(colors.Namespace, namespaceString)}" +
                      $"{WrapColor(colors.Class, classString)}" +
                      $"{WrapColor(colors.Method, methodString)}" +
                      $"{WrapColor(colors.Args, argsString)}" +
                      suffix;
        return true;
    }

    static string WrapColor(string color, string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        return $"<color={color}>{text}</color>";
    }

    static string FormatTime(long timeMsUtc)
    {
        if (timeMsUtc <= 0) return "--:--:--.---";
        return DateTimeOffset.FromUnixTimeMilliseconds(timeMsUtc)
            .ToLocalTime()
            .ToString(TimeFormat);
    }

    static string WrapHighlight(string source, int start, int length)
    {
        var color = EditorGUIUtility.isProSkin ? "#8FD6FF" : "#005A9E";
        return source.Substring(0, start)
               + $"<color={color}><b>"
               + source.Substring(start, length)
               + "</b></color>"
               + source.Substring(start + length);
    }

    Rect GetLinkRect(Rect lineRect, string line, int start, int length, GUIStyle style)
    {
        if (style == null) return default;
        if (start < 0 || length <= 0 || start + length > line.Length) return default;

        var prefix = line.Substring(0, start);
        var link = line.Substring(start, length);
        var prefixWidth = style.CalcSize(new GUIContent(prefix)).x;
        var linkWidth = style.CalcSize(new GUIContent(link)).x;

        if (prefixWidth >= lineRect.width || linkWidth <= 0f)
            return default;

        var width = Mathf.Min(linkWidth, lineRect.width - prefixWidth);
        if (width <= 1f) return default;

        return new Rect(lineRect.x + prefixWidth, lineRect.y, width, lineRect.height);
    }

    void SetHoverTooltip(Rect rect, string file, int lineNumber)
    {
        if (string.IsNullOrEmpty(file) || lineNumber <= 0)
            return;

        if (!rect.Contains(Event.current.mousePosition))
            return;

        var fullPath = ToFullPath(file);
        if (string.IsNullOrEmpty(fullPath))
            fullPath = file;
        if (string.IsNullOrEmpty(fullPath))
            return;

        _hoverTooltip = $"{fullPath}:{lineNumber}";
    }
}