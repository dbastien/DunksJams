using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

public static class DLog
{
    public static bool IsLoggingEnabled = Debug.isDebugBuild;
    public static bool IsTimestampEnabled = true;
    public static bool IsColorEnabled = true;

    public enum CallerInfoMode
    {
        None,
        Top, // fast: uses CallerFilePath/LineNumber
        Full // slow: StackTrace walk
    }

    public static CallerInfoMode CallerMode = CallerInfoMode.Top;

    public static bool IsTimingEnabled = true;
    public static bool IsFileLoggingEnabled = true;

    public static LogType FilterLogType
    {
        get => Debug.unityLogger.filterLogType;
        set => Debug.unityLogger.filterLogType = value;
    }

    public static bool UnityLoggerEnabled
    {
        get => Debug.unityLogger.logEnabled;
        set => Debug.unityLogger.logEnabled = value;
    }

    const string InfoColor = "#E6E6E6";
    const string WarningColor = "#FFCC00";
    const string ErrorColor = "#FF5555";
    const string ExceptionColor = "#FF55FF";
    const string TimeColor = "#80FFFF";
    const string CallerColor = "#90EE90";

    public interface ILogSink
    {
        void Log(LogType logType, string message, Object context);
    }

    sealed class ConsoleSink : ILogSink
    {
        public void Log(LogType logType, string message, Object context) =>
            Debug.unityLogger.Log(logType, (object)message, context);
    }

    sealed class FileSink : ILogSink
    {
        // Strip basic rich-text tags (color, link, bold) for file output.
        static readonly Regex s_stripTags = new(
            @"</?(color|a|b)(\s+[^>]*)?>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        readonly object _lock = new();
        readonly string _logFilePath;
        StreamWriter _writer;

        // Prevent recursive “file sink failed” loops.
        int _writeFailures;

        public FileSink()
        {
            var logDir = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            try
            {
                var fs = new FileStream(_logFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(fs, Encoding.UTF8) { AutoFlush = true };
            }
            catch
            {
                _writer = null;
            }
        }

        public void Log(LogType logType, string message, Object context)
        {
            if (!IsFileLoggingEnabled) return;
            if (_writer == null) return;

            var clean = s_stripTags.Replace(message ?? "", "");

            var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var lvl = logType.ToString().ToUpperInvariant();
            var ctx = context != null ? $" [{context.name}]" : "";

            lock (_lock)
            {
                try
                {
                    _writer.WriteLine($"[{ts}] [{lvl}]{ctx} {clean}");
                }
                catch
                {
                    _writeFailures++;
                    if (_writeFailures > 3)
                        IsFileLoggingEnabled = false;
                }
            }
        }

        public string GetLogFilePath() => _logFilePath;
    }

    static readonly List<ILogSink> s_sinks = new()
    {
        new ConsoleSink(),
        new FileSink()
    };

    // Hyperlink formatting
    // We use a custom scheme so we never collide with Unity’s own URL/path handlers.
    public static string DLogLink(string pathOrAssetPath, int lineNumber, string displayText = null)
    {
        var text = displayText ?? $"{Path.GetFileName(pathOrAssetPath)}:{lineNumber}";
        return $"<a href=\"dlog://{pathOrAssetPath}\" line=\"{lineNumber}\">{text}</a>";
    }

    public static string UrlLink(string url, string displayText = null)
        => $"<a href=\"{url}\">{displayText ?? url}</a>";

    // ----------------------------
    // Public API
    // ----------------------------
    [HideInStackTrace]
    public static void Log(string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        => LogImpl(LogType.Log, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogW(string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        => LogImpl(LogType.Warning, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogE(string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        => LogImpl(LogType.Error, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogException(Exception ex, Object ctx = null)
        => Debug.unityLogger.LogException(ex, ctx);

    [HideInStackTrace]
    public static void LogFormat(string format, params object[] args) =>
        Log(string.Format(format, args));

    [HideInStackTrace]
    public static void LogWFormat(string format, params object[] args) =>
        LogW(string.Format(format, args));

    [HideInStackTrace]
    public static void LogEFormat(string format, params object[] args) =>
        LogE(string.Format(format, args));

    [HideInStackTrace]
    public static void Time(Action action, string label = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();

        if (!IsTimingEnabled) return;

        // Respect output settings.
        if (!UnityLoggerEnabled && !IsFileLoggingEnabled) return;

        var where = label ?? $"{Path.GetFileName(file)}:{line} ({member})";
        LogTiming($"{where} took {sw.ElapsedMilliseconds}ms", null, true, file, line, member);
    }

    static string s_projectRoot;

    static string ProjectRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(s_projectRoot)) return s_projectRoot;

            s_projectRoot = Application.dataPath.Replace("\\", "/");
            if (s_projectRoot.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
                s_projectRoot = s_projectRoot.Substring(0, s_projectRoot.Length - "/Assets".Length);

            return s_projectRoot;
        }
    }

    [ThreadStatic] static StringBuilder t_sb;

    static StringBuilder SB
    {
        get
        {
            if (t_sb == null) t_sb = new StringBuilder(256);
            t_sb.Length = 0;
            return t_sb;
        }
    }

    static string GetMsgColor(LogType t) => t switch
    {
        LogType.Warning => WarningColor,
        LogType.Error => ErrorColor,
        LogType.Exception => ExceptionColor,
        _ => InfoColor
    };

    static string Colorize(string text, string color)
        => IsColorEnabled ? $"<color={color}>{text}</color>" : text;

    static void LogImpl(LogType type, string msg, Object ctx, bool timestamp,
        string file, int line, string member)
    {
        if (!IsLoggingEnabled) return;
        LogImplInternal(type, msg, ctx, timestamp, file, line, member);
    }

    // Used by timing (bypasses IsLoggingEnabled but respects output settings)
    static void LogTiming(string msg, Object ctx = null, bool timestamp = false,
        string file = "", int line = 0, string member = "")
    {
        if (!UnityLoggerEnabled && !IsFileLoggingEnabled) return;
        LogImplInternal(LogType.Log, msg, ctx, timestamp, file, line, member);
    }

    static void LogImplInternal(LogType type, string msg, Object ctx, bool timestamp,
        string file, int line, string member)
    {
        var sb = SB;

        if (timestamp && IsTimestampEnabled)
            sb.Append(Colorize("[", TimeColor))
                .Append(Colorize(DateTime.Now.ToString("HH:mm:ss.fff"), TimeColor))
                .Append(Colorize("] ", TimeColor));

        if (CallerMode != CallerInfoMode.None)
        {
            if (CallerMode == CallerInfoMode.Top)
                AppendTopCaller(sb, file, line, member);
            else
                AppendFullCaller(sb, file, line, member);
        }

        if (!string.IsNullOrEmpty(msg))
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(Colorize(msg, GetMsgColor(type)));
        }

        var formatted = sb.ToString();

        for (var i = 0; i < s_sinks.Count; i++)
            s_sinks[i].Log(type, formatted, ctx);
    }

    static void AppendTopCaller(StringBuilder sb, string file, int line, string member)
    {
        var assetOrPath = ToAssetOrPath(file);
        var ln = Mathf.Max(1, line);

        var display = $"{Path.GetFileName(assetOrPath)}:{ln}";
        var link = DLogLink(assetOrPath, ln, display);

        if (sb.Length > 0) sb.Append(" ");
        sb.Append(Colorize(link, CallerColor));

        if (!string.IsNullOrEmpty(member))
            sb.Append(" ")
                .Append(Colorize(member, CallerColor));
    }

    static void AppendFullCaller(StringBuilder sb, string callerFile, int callerLine, string callerMember)
    {
        var trace = new StackTrace(true);
        var frames = trace.GetFrames();
        if (frames == null || frames.Length == 0)
            return;

        var first = true;

        for (var i = 0; i < frames.Length; i++)
        {
            var f = frames[i];
            var m = f.GetMethod();
            if (m == null) continue;

            var file = f.GetFileName();
            if (string.IsNullOrEmpty(file)) continue;

            if (file.IndexOf("DLog.cs", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            var line = f.GetFileLineNumber();
            var wherePath = ToAssetOrPath(file);
            var whereLine = first && callerLine > 0 ? callerLine : Mathf.Max(1, line);

            var cls = m.DeclaringType != null ? m.DeclaringType.Name : "Unknown";
            var name = m.Name;

            if (!first) sb.Append(Colorize(" <- ", CallerColor));

            var display = $"{Path.GetFileName(wherePath)}:{whereLine}";
            var link = DLogLink(wherePath, whereLine, display);

            sb.Append(Colorize(link, CallerColor))
                .Append(Colorize($" {cls}.{name}()", CallerColor));

            first = false;

            // Cap the breadcrumb chain.
            if (i >= 12) break;
        }
    }

    static string ToAssetOrPath(string file)
    {
        if (string.IsNullOrEmpty(file))
            return "Unknown";

        var p = file.Replace("\\", "/");

        var assetsIndex = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
        if (assetsIndex >= 0)
            return p.Substring(assetsIndex);

        var root = ProjectRoot;
        if (!string.IsNullOrEmpty(root) && p.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            var rel = p.Substring(root.Length).TrimStart('/');
            if (!rel.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                rel = "Assets/" + rel;
            return rel;
        }

        return p; // absolute fallback
    }

#if UNITY_EDITOR
    public static void OpenConsole()
    {
        EditorApplication.ExecuteMenuItem("Window/DLog");
    }
#endif
}