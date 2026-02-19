using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class DLog
{
    public static bool IsLoggingEnabled = Debug.isDebugBuild;
    public static bool IsTimestampEnabled = true;
    public static bool IsColorEnabled = true;

    public enum CallerInfoMode
    {
        None,
        Top,
        Full
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

    private const string InfoColor = "#E6E6E6";
    private const string WarningColor = "#FFCC00";
    private const string ErrorColor = "#FF5555";
    private const string ExceptionColor = "#FF55FF";
    private const string TimeColor = "#80FFFF";
    private const string CallerColor = "#90EE90";

    public interface ILogSink
    {
        void Log(LogType logType, string message, Object context);
    }

    private sealed class ConsoleSink : ILogSink
    {
        public void Log(LogType logType, string message, Object context) =>
            Debug.unityLogger.Log(logType, (object)message, context);
    }

    private sealed class FileSink : ILogSink
    {
        private static readonly Regex s_stripTags = new(
            @"</?(color|a|b)(\s+[^>]*)?>",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly object _lock = new();
        private readonly string _logFilePath;
        private StreamWriter _writer;
        private int _writeFailures;

        public FileSink()
        {
            string logDir = Path.Combine(Application.persistentDataPath, "Logs");
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

            string clean = s_stripTags.Replace(message ?? "", "");
            var ts = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string lvl = logType.ToString().ToUpperInvariant();
            string ctx = context != null ? $" [{context.name}]" : "";

            lock (_lock)
            {
                try { _writer.WriteLine($"[{ts}] [{lvl}]{ctx} {clean}"); }
                catch
                {
                    _writeFailures++;
                    if (_writeFailures > 3)
                        IsFileLoggingEnabled = false;
                }
            }
        }

        public string GetLogFilePath() => _logFilePath;

        public void Dispose()
        {
            lock (_lock)
            {
                try { _writer?.Dispose(); }
                catch { /* ignore */ }
                _writer = null;
            }
        }
    }

    private static readonly List<ILogSink> s_sinks = new(2);
    private static FileSink s_fileSink;

    private struct Pending
    {
        public LogType type;
        public string msg;
        public Object ctx;
    }

    private const int PendingCap = 128;
    private static readonly Queue<Pending> s_pending = new(PendingCap);
    private static bool s_inited;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void SubsystemReset()
    {
        s_inited = false;
        s_pending.Clear();

        if (s_fileSink != null)
        {
            s_fileSink.Dispose();
            s_fileSink = null;
        }

        s_sinks.Clear();
        s_projectRoot = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void EarlyInit()
    {
        EnsureInitialized();
        FlushPending();
    }

    public static string DLogLink(string pathOrAssetPath, int lineNumber, string displayText = null)
    {
        string text = displayText ?? $"{Path.GetFileName(pathOrAssetPath)}:{lineNumber}";
        // IMPORTANT: don’t URI-escape this; Unity/editor click handlers often expect raw paths.
        return $"<a href=\"dlog://{pathOrAssetPath}\" line=\"{lineNumber}\">{text}</a>";
    }

    public static string UrlLink(string url, string displayText = null)
        => $"<a href=\"{url}\">{displayText ?? url}</a>";

    [HideInStackTrace]
    public static void Log(
        string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = ""
    ) => LogImpl(LogType.Log, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogW(
        string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = ""
    ) => LogImpl(LogType.Warning, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogE(
        string msg, Object ctx = null, bool timestamp = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = ""
    ) => LogImpl(LogType.Error, msg, ctx, timestamp, file, line, member);

    [HideInStackTrace]
    public static void LogException(Exception ex, Object ctx = null) =>
        Debug.unityLogger.LogException(ex, ctx);

    [HideInStackTrace]
    public static void Time(
        Action action, string label = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = ""
    )
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();

        if (!IsTimingEnabled) return;
        if (!UnityLoggerEnabled && !IsFileLoggingEnabled) return;

        string where = label ?? $"{Path.GetFileName(file)}:{line} ({member})";
        LogTiming($"{where} took {sw.ElapsedMilliseconds}ms", null, true, file, line, member);
    }

    private static void EnsureInitialized()
    {
        if (s_inited) return;

        s_sinks.Clear();
        s_sinks.Add(new ConsoleSink());

        if (IsFileLoggingEnabled)
        {
            try
            {
                s_fileSink = new FileSink();
                s_sinks.Add(s_fileSink);
            }
            catch
            {
                s_fileSink = null;
            }
        }

        s_inited = true;
    }

    private static void EnqueuePending(LogType type, string formatted, Object ctx)
    {
        if (s_pending.Count >= PendingCap)
            s_pending.Dequeue();

        s_pending.Enqueue(new Pending { type = type, msg = formatted, ctx = ctx });
    }

    private static void FlushPending()
    {
        if (!s_inited) return;
        while (s_pending.Count > 0)
        {
            var p = s_pending.Dequeue();
            for (var i = 0; i < s_sinks.Count; i++)
                s_sinks[i].Log(p.type, p.msg, p.ctx);
        }
    }

    private static string s_projectRoot;

    private static string ProjectRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(s_projectRoot)) return s_projectRoot;

            s_projectRoot = Application.dataPath.Replace("\\", "/");
            if (s_projectRoot.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
                s_projectRoot = s_projectRoot[..^"/Assets".Length];

            return s_projectRoot;
        }
    }

    [ThreadStatic] private static StringBuilder t_sb;

    private static StringBuilder SB
    {
        get
        {
            t_sb ??= new StringBuilder(256);
            t_sb.Length = 0;
            return t_sb;
        }
    }

    private static string GetMsgColor(LogType t) => t switch
    {
        LogType.Warning => WarningColor,
        LogType.Error => ErrorColor,
        LogType.Exception => ExceptionColor,
        _ => InfoColor
    };

    private static string Colorize(string text, string color)
        => IsColorEnabled ? $"<color={color}>{text}</color>" : text;

    private static void LogImpl(
        LogType type, string msg, Object ctx, bool timestamp,
        string file, int line, string member
    )
    {
        if (!IsLoggingEnabled) return;
        if (!UnityLoggerEnabled && !IsFileLoggingEnabled) return;

        EnsureInitialized();
        LogImplInternal(type, msg, ctx, timestamp, file, line, member);
    }

    private static void LogTiming(
        string msg, Object ctx = null, bool timestamp = false,
        string file = "", int line = 0, string member = ""
    )
    {
        if (!UnityLoggerEnabled && !IsFileLoggingEnabled) return;

        EnsureInitialized();
        LogImplInternal(LogType.Log, msg, ctx, timestamp, file, line, member);
    }

    private static void LogImplInternal(
        LogType type, string msg, Object ctx, bool timestamp,
        string file, int line, string member
    )
    {
        StringBuilder sb = SB;

        if (timestamp && IsTimestampEnabled)
        {
            sb.Append(Colorize("[", TimeColor))
              .Append(Colorize(DateTime.Now.ToString("HH:mm:ss.fff"), TimeColor))
              .Append(Colorize("] ", TimeColor));
        }

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

        if (!s_inited || s_sinks.Count == 0)
        {
            EnqueuePending(type, formatted, ctx);
            return;
        }

        for (var i = 0; i < s_sinks.Count; i++)
            s_sinks[i].Log(type, formatted, ctx);
    }

    private static void AppendTopCaller(StringBuilder sb, string file, int line, string member)
    {
        string assetOrPath = ToAssetOrPath(file);
        int ln = Mathf.Max(1, line);

        var display = $"{Path.GetFileName(assetOrPath)}:{ln}";
        string link = DLogLink(assetOrPath, ln, display);

        if (sb.Length > 0) sb.Append(" ");
        sb.Append(Colorize(link, CallerColor));

        if (!string.IsNullOrEmpty(member))
            sb.Append(" ").Append(Colorize(member, CallerColor));
    }

    private static void AppendFullCaller(StringBuilder sb, string callerFile, int callerLine, string callerMember)
    {
        var trace = new StackTrace(true);
        StackFrame[] frames = trace.GetFrames();
        if (frames == null || frames.Length == 0) return;

        var first = true;

        for (var i = 0; i < frames.Length; i++)
        {
            StackFrame f = frames[i];
            MethodBase m = f.GetMethod();
            if (m == null) continue;

            string file = f.GetFileName();
            if (string.IsNullOrEmpty(file)) continue;

            if (file.IndexOf("DLog.cs", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            int line = f.GetFileLineNumber();
            string wherePath = ToAssetOrPath(file);
            int whereLine = first && callerLine > 0 ? callerLine : Mathf.Max(1, line);

            string cls = m.DeclaringType != null ? m.DeclaringType.Name : "Unknown";
            string name = m.Name;

            if (!first) sb.Append(Colorize(" <- ", CallerColor));

            var display = $"{Path.GetFileName(wherePath)}:{whereLine}";
            string link = DLogLink(wherePath, whereLine, display);

            sb.Append(Colorize(link, CallerColor))
              .Append(Colorize($" {cls}.{name}()", CallerColor));

            first = false;
            if (i >= 12) break;
        }
    }

    private static string ToAssetOrPath(string file)
    {
        if (string.IsNullOrEmpty(file)) return "Unknown";

        string p = file.Replace("\\", "/");

        int assetsIndex = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
        if (assetsIndex >= 0)
            return p[assetsIndex..];

        string root = ProjectRoot;
        if (!string.IsNullOrEmpty(root) && p.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            string rel = p[root.Length..].TrimStart('/');
            if (!rel.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                rel = "Assets/" + rel;
            return rel;
        }

        return p;
    }

#if UNITY_EDITOR
    public static void OpenConsole()
    {
        EditorApplication.ExecuteMenuItem("‽/DLog/Window");
    }
#endif
}