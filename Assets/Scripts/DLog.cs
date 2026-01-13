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

public static class DLog
{
    //todo: use SettingsProvider?
    public static bool IsLoggingEnabled = Debug.isDebugBuild;
    public static bool IsTimestampEnabled = true;
    public static bool IsColorEnabled = true;
    public static bool IsCallerInfoEnabled = true;
    
    //todo: better colors
    static readonly string InfoColor = "#FFFFFF";
    static readonly string WarningColor = "#FFCC00"; // Soft yellow
    static readonly string ErrorColor = "#FF5555";   // Bright red
    static readonly string ExceptionColor = "#FF00FF"; // Magenta
    static readonly string TimeColor = "#80FFFF";
    static readonly string CallerColor = "#90EE90";
    
    static readonly ILogger Logger = Debug.unityLogger;

    public static bool IsFileLoggingEnabled = true;
    
    //todo: BufferedSink, AnalyticsSink, RemoteSink
    static readonly List<ILogSink> LogSinks = new() { new ConsoleSink(), new FileSink() };
    
    public interface ILogSink
    {
        void Log(LogType logType, string message, Object context);
    }
    
    public class ConsoleSink : ILogSink
    {
        public void Log(LogType logType, string msg, Object context) => 
            Debug.unityLogger.Log(logType, (object)msg, context);
    }
    
    public class FileSink : ILogSink
    {
        static readonly Regex ColorTagRegex = new(@"<color=[^>]*>|</color>", RegexOptions.Compiled);
        readonly string _logFilePath;
        readonly object _lock = new();
        
        public FileSink()
        {
            string logDir = Path.Combine(Application.persistentDataPath, "Logs");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        }
        
        public void Log(LogType logType, string msg, Object context)
        {
            if (!IsFileLoggingEnabled) return;
            
            // Strip color tags for file output
            string cleanMsg = ColorTagRegex.Replace(msg, "");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logLevel = logType.ToString().ToUpper();
            string contextName = context != null ? $" [{context.name}]" : "";
            
            string line = $"[{timestamp}] [{logLevel}]{contextName} {cleanMsg}";
            
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Fallback to console if file write fails - avoid infinite loop
                    Debug.LogWarning($"FileSink failed to write: {ex.Message}");
                }
            }
        }
        
        public string GetLogFilePath() => _logFilePath;
    }
    
    [HideInStackTrace]
    public static void Log(string msg, Object ctx = null, bool timestamp = false, bool fullStack = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") =>
        LogImpl(LogType.Log, msg, ctx, timestamp, fullStack, file, line, member);
    
    [HideInStackTrace]
    public static void LogW(string msg, Object ctx = null, bool timestamp = false, bool fullStack = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") =>
        LogImpl(LogType.Warning, msg, ctx, timestamp, fullStack, file, line, member);
    
    [HideInStackTrace]
    public static void LogE(string msg, Object ctx = null, bool timestamp = false, bool fullStack = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") => 
        LogImpl(LogType.Error, msg, ctx, timestamp, fullStack, file, line, member);
    
    [HideInStackTrace]
    public static void LogException(Exception ex, Object ctx = null) => Logger.LogException(ex, ctx);
    
    [HideInStackTrace]
    public static void LogExceptionComplete(Exception ex, Object ctx = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "") => 
        LogImpl(LogType.Exception, FormatExceptionComplete(ex), ctx, timestamp: true, fullStack: true, file, line, member);
    
    //handles inner exceptions as well
    [HideInStackTrace]
    public static string FormatExceptionComplete(Exception ex)
    {
        var builder = new StringBuilder()
            .AppendLine($"{ex.GetType()}: {ex.Message}")
            .AppendLine(ex.StackTrace);

        if (ex.InnerException != null)
            builder.AppendLine("Inner Exception:")
                .AppendLine(FormatExceptionComplete(ex.InnerException));

        return builder.ToString().Trim();
    }

    [HideInStackTrace]
    static void LogImpl(LogType logType, string msg, Object ctx, bool timestamp, bool fullStack,
        string file, int line, string member)
    {
        if (!IsLoggingEnabled) return;
        
        string caller = "";
        if (IsCallerInfoEnabled)
        {
            caller = fullStack 
                ? GetFullStackInfo() 
                : $"{Path.GetFileName(file)}:{line} ({member})";
        }
        
        string time = timestamp && IsTimestampEnabled ? $"[{DateTime.Now:HH:mm:ss}] " : "";
     
        string msgFormatted = $"{Colorize(time, TimeColor)}{Colorize(caller, CallerColor)} {Colorize(msg, GetColor(logType))}";
        
        foreach (ILogSink sink in LogSinks) 
            sink.Log(logType, msgFormatted, ctx);
    }

    [HideInStackTrace]
    static string GetFullStackInfo()
    {
        var trace = new StackTrace(true);
        var sb = new StringBuilder();
        bool first = true;
        
        foreach (var frame in trace.GetFrames())
        {
            var fileName = frame.GetFileName();
            var method = frame.GetMethod();

            if (fileName != null &&
                !fileName.Contains("DLog.cs") &&
                method.GetCustomAttribute<HideInStackTraceAttribute>() == null)
            {
                if (!first) sb.Append(" <- ");
                sb.Append($"{Path.GetFileName(fileName)}:{frame.GetFileLineNumber()} ({method.Name})");
                first = false;
            }
        }
        return sb.ToString();
    }
    
    [HideInStackTrace]
    static string Colorize(string text, string color) => 
        IsColorEnabled ? $"<color={color}>{text}</color>" : text;
    
    [HideInStackTrace]
    static string GetColor(LogType logType) => logType switch
    {
        LogType.Warning => WarningColor,
        LogType.Error => ErrorColor,
        LogType.Exception => ExceptionColor,
        _ => InfoColor
    };

    [HideInStackTrace]
    public static void Time(Action action, string label = null,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
    {
        string timingLabel = label ?? $"{Path.GetFileName(file)}:{line} ({member})";
        var stopwatch = Stopwatch.StartNew();

        action();
        stopwatch.Stop();

        Log($"{timingLabel} took {stopwatch.ElapsedMilliseconds}ms", timestamp: true);
    }
}