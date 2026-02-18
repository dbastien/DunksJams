// Assets/Editor/DLog/DLogConsole.Parsing.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed partial class DLogConsole
{
    private static readonly Regex s_stackAtFileLine = new(@"\(at\s+(.+?):(\d+)\)", RegexOptions.Compiled);
    private static readonly Regex s_stackInLine = new(@"\s+in\s+(.+):line\s+(\d+)\s*$", RegexOptions.Compiled);
    private static readonly Regex s_stackLooseFileLine = new(@"(.+?\.\w+):(\d+)", RegexOptions.Compiled);
    private static readonly Regex s_bracketFileLine = new(@"\[(.+?):(\d+)\]", RegexOptions.Compiled);
    private static readonly Regex s_richTextTags = new(@"</?(color|a|b)(\s+[^>]*)?>", RegexOptions.Compiled);

    private static string s_projectRoot;

    private static readonly StackColors s_stackColorsPro = new()
    {
        Namespace = "#9BB3CE",
        Class = "#5CB5FF",
        Method = "#86D6FF",
        Args = "#B6CCE0"
    };

    private static readonly StackColors s_stackColorsLight = new()
    {
        Namespace = "#5E6C86",
        Class = "#2B7EB6",
        Method = "#4A7CB0",
        Args = "#7A889E"
    };

    private struct StackColors
    {
        public string Namespace;
        public string Class;
        public string Method;
        public string Args;
    }

    private void EnsureMessageCache(LogEntry e)
    {
        if (e == null || e.messageLinkParsed)
            return;

        string raw = e.richMessage ?? e.message ?? "";
        e.plainMessage = StripRichText(raw);
        e.messageLinkParsed = true;
        e.messageLink = default;

        if (string.IsNullOrEmpty(e.plainMessage))
            return;

        if (TryGetMessageLinkSpan(e.plainMessage, out string file, out int lineNumber, out int start, out int length))
            e.messageLink = new LinkSpan
            {
                hasLink = true,
                file = file,
                lineNumber = lineNumber,
                start = start,
                length = length
            };
    }

    private void EnsureStackCache(LogEntry e)
    {
        if (e == null)
            return;

        bool isProSkin = EditorGUIUtility.isProSkin;
        if (e.stackCacheValid && e.stackCacheProSkin == isProSkin)
            return;

        e.stackCacheValid = true;
        e.stackCacheProSkin = isProSkin;
        e.stackLineInfos = null;
        e.stackScrollHeight = 0f;

        if (string.IsNullOrEmpty(e.stackTrace))
            return;

        string[] lines = e.stackTrace.Split('\n');
        var infos = new List<StackLineInfo>(lines.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var info = new StackLineInfo { line = line };
            if (TryGetStackLinkSpan(line, out string file, out int lineNumber, out int start, out int length))
                info.link = new LinkSpan
                {
                    hasLink = true,
                    file = file,
                    lineNumber = lineNumber,
                    start = start,
                    length = length
                };

            if (TryBuildStackDisplayLine(line, info.link, isProSkin, out string displayLine))
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

        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        int linesToShow = Mathf.Min(MaxStackLinesVisible, e.stackLineInfos.Length);
        e.stackScrollHeight = Mathf.Min(MaxStackHeight, lineHeight * linesToShow);
    }

    private static bool TryParseStackLine(string line, out string file, out int lineNumber)
    {
        file = null;
        lineNumber = 0;

        Match m = s_stackAtFileLine.Match(line);
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

    private static string ToAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        string p = path.Replace("\\", "/");
        int idx = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? p[idx..] : p;
    }

    private static string ProjectRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(s_projectRoot)) return s_projectRoot;

            string dataPath = Application.dataPath.Replace("\\", "/");
            if (dataPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
                s_projectRoot = dataPath[..^"/Assets".Length];
            else
                s_projectRoot = Path.GetDirectoryName(dataPath) ?? dataPath;

            return s_projectRoot;
        }
    }

    private static string ToFullPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        string p = path.Replace("\\", "/");
        if (Path.IsPathRooted(p))
            return p;

        string root = ProjectRoot;
        if (string.IsNullOrEmpty(root))
            return p;

        return Path.Combine(root, p).Replace("\\", "/");
    }

    private bool TryGetMessageLinkSpan(string line, out string file, out int lineNumber, out int start, out int length)
    {
        if (TryGetStackLinkSpan(line, out file, out lineNumber, out start, out length))
            return true;

        Match match = s_bracketFileLine.Match(line);
        return TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length);
    }

    private bool TryGetStackLinkSpan(string line, out string file, out int lineNumber, out int start, out int length)
    {
        file = null;
        lineNumber = 0;
        start = 0;
        length = 0;

        if (string.IsNullOrEmpty(line))
            return false;

        Match match = s_stackAtFileLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackInLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackLooseFileLine.Match(line);
        return TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length);
    }

    private bool TryBuildLinkSpan
    (
        Match match, string line, out string file, out int lineNumber, out int start,
        out int length
    )
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
        try { fileName = Path.GetFileName(file); }
        catch (ArgumentException) { return false; }

        if (string.IsNullOrEmpty(fileName))
            return false;

        int pathStart = match.Groups[1].Index;
        int offset = file.Replace("\\", "/").LastIndexOf('/');
        if (offset >= 0) { offset += 1; }
        else
        {
            offset = file.LastIndexOf('\\');
            offset = offset >= 0 ? offset + 1 : 0;
        }

        start = pathStart + offset;
        var token = $"{fileName}:{lineNumber}";
        length = token.Length;

        // If the line uses ":line N", expand to include it for readability.
        int lineTokenIndex = line.IndexOf($"{fileName}:line {lineNumber}", StringComparison.OrdinalIgnoreCase);
        if (lineTokenIndex >= 0)
        {
            start = lineTokenIndex;
            length = $"{fileName}:line {lineNumber}".Length;
        }

        if (start < 0 || start + length > line.Length)
            return false;

        return true;
    }

    private static bool TryGetFileName(string path, out string fileName)
    {
        fileName = null;
        if (string.IsNullOrEmpty(path))
            return false;
        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            return false;

        try { fileName = Path.GetFileName(path); }
        catch (ArgumentException) { return false; }

        return !string.IsNullOrEmpty(fileName);
    }

    private static string StripRichText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return s_richTextTags.Replace(text, "");
    }

    private static bool TryBuildStackDisplayLine(string line, LinkSpan link, bool isProSkin, out string displayLine)
    {
        displayLine = null;
        if (string.IsNullOrEmpty(line))
            return false;

        int methodLastIndex = line.IndexOf('(');
        if (methodLastIndex <= 0)
            return false;

        int argsLastIndex = line.IndexOf(')', methodLastIndex);
        if (argsLastIndex <= 0)
            return false;

        int methodFirstIndex = line.LastIndexOf(':', methodLastIndex);
        if (methodFirstIndex <= 0)
            methodFirstIndex = line.LastIndexOf('.', methodLastIndex);
        if (methodFirstIndex <= 0)
            return false;

        string methodString = line.Substring(methodFirstIndex + 1, methodLastIndex - methodFirstIndex - 1);

        string classString;
        var namespaceString = string.Empty;
        int classFirstIndex = line.LastIndexOf('.', methodFirstIndex - 1);
        if (classFirstIndex <= 0) { classString = line[..(methodFirstIndex + 1)]; }
        else
        {
            classString = line.Substring(classFirstIndex + 1, methodFirstIndex - classFirstIndex);
            namespaceString = line[..(classFirstIndex + 1)];
        }

        string argsString = line.Substring(methodLastIndex, argsLastIndex - methodLastIndex + 1);
        int suffixStart = argsLastIndex + 1;
        string suffix = suffixStart < line.Length ? line[suffixStart..] : string.Empty;

        if (link.hasLink && suffix.Length > 0)
        {
            int linkStartInSuffix = link.start - suffixStart;
            if (linkStartInSuffix >= 0 && linkStartInSuffix + link.length <= suffix.Length)
                suffix = WrapHighlight(suffix, linkStartInSuffix, link.length);
        }

        StackColors colors = isProSkin ? s_stackColorsPro : s_stackColorsLight;
        displayLine = $"{WrapColor(colors.Namespace, namespaceString)}" +
                      $"{WrapColor(colors.Class, classString)}" +
                      $"{WrapColor(colors.Method, methodString)}" +
                      $"{WrapColor(colors.Args, argsString)}" +
                      suffix;
        return true;
    }

    private static string WrapColor(string color, string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        return $"<color={color}>{text}</color>";
    }

    private static string FormatTime(long timeMsUtc)
    {
        if (timeMsUtc <= 0) return "--:--:--.---";
        return DateTimeOffset.FromUnixTimeMilliseconds(timeMsUtc).ToLocalTime().ToString(TimeFormat);
    }

    private static string WrapHighlight(string source, int start, int length)
    {
        string color = EditorGUIUtility.isProSkin ? "#8FD6FF" : "#005A9E";
        return source[..start] +
               $"<color={color}><b>" +
               source.Substring(start, length) +
               "</b></color>" +
               source[(start + length)..];
    }

    private Rect GetLinkRect(Rect lineRect, string line, int start, int length, GUIStyle style)
    {
        if (style == null) return default;
        if (start < 0 || length <= 0 || start + length > line.Length) return default;

        string prefix = line[..start];
        string link = line.Substring(start, length);
        float prefixWidth = style.CalcSize(new GUIContent(prefix)).x;
        float linkWidth = style.CalcSize(new GUIContent(link)).x;

        if (prefixWidth >= lineRect.width || linkWidth <= 0f)
            return default;

        float width = Mathf.Min(linkWidth, lineRect.width - prefixWidth);
        if (width <= 1f) return default;

        return new Rect(lineRect.x + prefixWidth, lineRect.y, width, lineRect.height);
    }

    private void SetHoverTooltip(Rect rect, string file, int lineNumber)
    {
        if (string.IsNullOrEmpty(file) || lineNumber <= 0)
            return;

        if (!rect.Contains(Event.current.mousePosition))
            return;

        string fullPath = ToFullPath(file);
        if (string.IsNullOrEmpty(fullPath))
            fullPath = file;
        if (string.IsNullOrEmpty(fullPath))
            return;

        _hoverTooltip = $"{fullPath}:{lineNumber}";
    }
}