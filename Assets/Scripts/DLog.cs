using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Unity.Logging;
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
    
    static readonly List<string> IgnoredMethods = new()
    {
        "UnityEngine.DebugLogHandler",
        "DLog",
        "System.Diagnostics.StackTrace"
    };
    
    //todo: better colors
    static readonly string InfoColor = "#FFFFFF";
    static readonly string WarningColor = "#FFCC00"; // Soft yellow
    static readonly string ErrorColor = "#FF5555";   // Bright red
    static readonly string ExceptionColor = "#FF00FF"; // Magenta
    static readonly string TimeColor = "#80FFFF";
    static readonly string CallerColor = "#90EE90";
    
    static readonly ILogger Logger = Debug.unityLogger;

    //todo: BufferedSink, AnalyticsSink, RemoteSink, FileSink
    static readonly List<ILogSink> LogSinks = new() { new ConsoleSink() };
    public interface ILogSink
    {
        public void Log(LogType logType, string message, Object context);
    }
    public class ConsoleSink : ILogSink
    {
        public void Log(LogType logType, string msg, Object context) => Debug.unityLogger.Log(logType, (object)msg, context);
    }
    
    //public static void Log(object message) => Debug.unityLogger.Log(LogType.Log, message);

    [HideInStackTrace]
    public static void Log(string msg, Object ctx = null, bool timestamp = false) =>
        Log(LogType.Log, msg, ctx, timestamp);
    
    [HideInStackTrace]
    public static void LogW(string msg, Object ctx = null, bool timestamp = false) =>
        LogInternal(LogType.Warning, msg, ctx, timestamp);
    
    [HideInStackTrace]
    public static void LogE(string msg, Object ctx = null, bool timestamp = false) => 
        Log(LogType.Error, msg, ctx, timestamp);
    
    [HideInStackTrace]
    public static void LogException(Exception ex, Object ctx = null) => Logger.LogException(ex, ctx);
    
    [HideInStackTrace]
    public static void LogExceptionComplete(Exception ex, Object ctx = null) => 
        Log(LogType.Exception, FormatExceptionComplete(ex), ctx, timestamp: true);
    
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
    static void Log(LogType logType, string msg, Object ctx, bool timestamp)
    {
        if (!IsLoggingEnabled) return;
        
        string caller = IsCallerInfoEnabled ? GetCallerInfo() : "";
        string time = timestamp && IsTimestampEnabled ? $"[{DateTime.Now:HH:mm:ss}] " : "";
     
        string msgFormatted = $"{Colorize(time, TimeColor)} {Colorize(caller, CallerColor)} {Colorize(msg, GetColor(logType))}";
        Logger.Log(logType, (object)msgFormatted, ctx);
        //foreach (ILogSink sink in LogSinks) sink.Log(logType, msg, context);
    }
    
    [HideInStackTrace]
    static void LogInternal(LogType logType, string msg, Object ctx, bool timestamp)
    {
        string callerInfo = IsCallerInfoEnabled ? Colorize(GetCallerInfo(), CallerColor) : "";
        string time = timestamp && IsTimestampEnabled ? Colorize($"[{DateTime.Now:HH:mm:ss}] ", TimeColor) : "";
        string message = Colorize(msg, GetColor(logType));
    
        string msgFormatted = $"{time}{callerInfo} {message}";

        Debug.unityLogger.logHandler.LogFormat(logType, ctx, "{0}", msgFormatted);
    }

    [HideInStackTrace]
    static string GetCallerInfo()
    {
        var trace = new StackTrace(true);
        foreach (var frame in trace.GetFrames())
        {
            var fileName = frame.GetFileName();
            var method = frame.GetMethod();

            if (fileName != null &&
                !fileName.Contains("DLog.cs") &&
                method.GetCustomAttribute<HideInStackTrace>() == null)
            {
                return $"{System.IO.Path.GetFileName(fileName)}:{frame.GetFileLineNumber()} ({frame.GetMethod().Name})";
            }
        }
        return "";
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
    public static void Time(Action action, string label = null)
    {
        string timingLabel = label ?? GetCallerInfo();
        var stopwatch = Stopwatch.StartNew();

        action();
        stopwatch.Stop();

        Log($"{timingLabel} took {stopwatch.ElapsedMilliseconds}ms", timestamp: true);
    }
}