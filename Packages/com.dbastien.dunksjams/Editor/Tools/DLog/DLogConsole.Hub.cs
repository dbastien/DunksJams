// Assets/Editor/DLog/DLogConsole.Hub.cs

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public sealed partial class DLogConsole
{
    [Serializable]
    sealed class LogDump
    {
        public List<LogEntry> entries = new();
        public int version;
    }

    [InitializeOnLoad]
    static class DLogHub
    {
        const int MaxEntries = 2000;
        const string SessionKey = "DLogConsole.LogDump.v3";

        static readonly ConcurrentQueue<LogEntry> s_pending = new();
        static readonly List<LogEntry> s_entries = new(1024);

        static int s_version;
        static bool s_dirty;

        static int s_infoCount;
        static int s_warnCount;
        static int s_errorCount;

        const string ClearOnPlayKey = "DLogConsole.ClearOnPlay";
        const string ClearOnBuildKey = "DLogConsole.ClearOnBuild";
        const string ClearOnRecompileKey = "DLogConsole.ClearOnRecompile";

        static bool s_clearOnPlay;
        static bool s_clearOnBuild;
        static bool s_clearOnRecompile;

        // If Unity prints compiler diagnostics to console, skip those (we capture from CompilationPipeline).
        static readonly Regex s_compilerConsoleLine = new(
            @"\(\d+,\d+\):\s*(warning|error)\s+CS\d+:",
            RegexOptions.Compiled);

        // Highlight patterns like [file:123] or file.cs:123 (best-effort)
        static readonly Regex s_linkLike = new(
            @"(\[[^\]]+:\d+\])|(\b[\w\./\\-]+\.\w+:\d+\b)",
            RegexOptions.Compiled);

        static readonly Regex s_compilationFileLine =
            new(@"^\[COMPILATION\]\s+(.+?)\((\d+)\):", RegexOptions.Compiled);

        static DLogHub()
        {
            s_clearOnPlay = EditorPrefs.GetBool(ClearOnPlayKey, false);
            s_clearOnBuild = EditorPrefs.GetBool(ClearOnBuildKey, false);
            s_clearOnRecompile = EditorPrefs.GetBool(ClearOnRecompileKey, false);

            LoadFromSession();

            // Avoid accidental double-subscribe in weird Unity cycles:
            Application.logMessageReceivedThreaded -= OnLogThreaded;
            Application.logMessageReceivedThreaded += OnLogThreaded;

            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationStarted += OnCompilationStarted;

            EditorApplication.update -= FlushOnUpdate;
            EditorApplication.update += FlushOnUpdate;

            AssemblyReloadEvents.beforeAssemblyReload -= FlushAndSave;
            AssemblyReloadEvents.beforeAssemblyReload += FlushAndSave;

            EditorApplication.quitting -= FlushAndSave;
            EditorApplication.quitting += FlushAndSave;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static int Version => s_version;
        public static IReadOnlyList<LogEntry> Entries => s_entries;

        public static int InfoCount => s_infoCount;
        public static int WarningCount => s_warnCount;
        public static int ErrorCount => s_errorCount;

        public static event Action Changed;

        internal static bool ClearOnPlay
        {
            get => s_clearOnPlay;
            set
            {
                if (s_clearOnPlay == value) return;
                s_clearOnPlay = value;
                EditorPrefs.SetBool(ClearOnPlayKey, value);
            }
        }

        internal static bool ClearOnBuild
        {
            get => s_clearOnBuild;
            set
            {
                if (s_clearOnBuild == value) return;
                s_clearOnBuild = value;
                EditorPrefs.SetBool(ClearOnBuildKey, value);
            }
        }

        internal static bool ClearOnRecompile
        {
            get => s_clearOnRecompile;
            set
            {
                if (s_clearOnRecompile == value) return;
                s_clearOnRecompile = value;
                EditorPrefs.SetBool(ClearOnRecompileKey, value);
            }
        }

        public static void Clear()
        {
            s_entries.Clear();
            while (s_pending.TryDequeue(out _))
            {
            }

            s_infoCount = s_warnCount = s_errorCount = 0;

            s_version++;
            s_dirty = true;
            Changed?.Invoke();
        }

        static void OnLogThreaded(string message, string stackTrace, LogType type)
        {
            // Skip compiler messages emitted to console if we capture via CompilationPipeline.
            if (!string.IsNullOrEmpty(message) && s_compilerConsoleLine.IsMatch(message))
                return;

            s_pending.Enqueue(MakeEntry(message, stackTrace, type));
        }

        static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (messages == null || messages.Length == 0)
                return;

            // Only warnings/errors (info spam is not useful)
            for (var i = 0; i < messages.Length; i++)
            {
                var m = messages[i];
                if (m.type != CompilerMessageType.Warning && m.type != CompilerMessageType.Error)
                    continue;

                var lt = m.type == CompilerMessageType.Warning ? LogType.Warning : LogType.Error;

                var msg = BuildCompilationMessage(m, out var file, out var line, out var richMessage);

                var e = MakeEntry(msg, "", lt);
                e.file = file;
                e.line = line;
                e.richMessage = richMessage;

                s_pending.Enqueue(e);
            }
        }

        static void OnCompilationStarted(object obj)
        {
            if (s_clearOnRecompile)
                Clear();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!s_clearOnPlay)
                return;

            if (state == PlayModeStateChange.ExitingEditMode)
                Clear();
        }

        static LogEntry MakeEntry(string message, string stackTrace, LogType type)
        {
            var e = new LogEntry
            {
                message = message ?? "",
                stackTrace = stackTrace ?? "",
                type = type,
                timeMsUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Pre-format once (rich text highlight)
            // Use RGB only to avoid alpha weirdness across versions.
            e.richMessage = s_linkLike.Replace(e.message, "<color=#00FFFF><b>$0</b></color>");

            // Try to pre-extract a location (best effort)
            TryExtractFileLine(e.message, e.stackTrace, out e.file, out e.line);

            return e;
        }

        static string BuildCompilationMessage(CompilerMessage m, out string file, out int line,
            out string richMessage)
        {
            file = m.file ?? "";
            line = Mathf.Max(0, m.line);

            var description = StripCompilationPrefix(m.message);
            var displayFile = ToAssetPath(file);
            var lineSegment = line > 0 ? $"({line})" : "";
            var prefix = string.IsNullOrEmpty(displayFile)
                ? "[COMPILATION]"
                : $"[COMPILATION] {displayFile}{lineSegment}:";

            var msg = string.IsNullOrEmpty(description) ? prefix : $"{prefix} {description}";
            richMessage = ColorizePrefix(m.type, prefix, description);

            return msg;
        }

        static string StripCompilationPrefix(string message)
        {
            if (string.IsNullOrEmpty(message)) return "";

            var idx = message.IndexOf("):", StringComparison.Ordinal);
            if (idx >= 0 && idx + 2 < message.Length)
            {
                var start = idx + 2;
                if (message[start] == ' ') start++;
                return message.Substring(start);
            }

            return message;
        }

        static string ToAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            var p = path.Replace("\\", "/");
            var idx = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
            return idx >= 0 ? p.Substring(idx) : p;
        }

        static string ColorizePrefix(CompilerMessageType type, string prefix, string description)
        {
            var color = type == CompilerMessageType.Warning
                ? "#FFCC00"
                : "#FF5555";

            if (string.IsNullOrEmpty(prefix)) return description ?? "";
            if (string.IsNullOrEmpty(description)) return $"<color={color}>{prefix}</color>";
            return $"<color={color}>{prefix}</color> {description}";
        }

        static void FlushOnUpdate()
        {
            if (s_pending.IsEmpty)
                return;

            var changed = false;

            while (s_pending.TryDequeue(out var e))
            {
                // Consecutive dedupe: if same as last, bump count instead of adding a new row
                if (s_entries.Count > 0)
                {
                    var last = s_entries[s_entries.Count - 1];
                    if (last.type == e.type &&
                        last.message == e.message &&
                        last.stackTrace == e.stackTrace)
                    {
                        last.count++;
                        last.timeMsUtc = e.timeMsUtc;

                        // Increment global counts by 1 occurrence
                        IncrementTypeCounts(e.type, 1);

                        changed = true;
                        continue;
                    }
                }

                s_entries.Add(e);
                IncrementTypeCounts(e.type, e.count);
                changed = true;
            }

            if (!changed)
                return;

            // Trim in one shot. If we trimmed, recompute counts (simple + safe).
            if (s_entries.Count > MaxEntries)
            {
                var remove = s_entries.Count - MaxEntries;
                s_entries.RemoveRange(0, remove);
                RecomputeCounts();
            }

            s_version++;
            s_dirty = true;

            Changed?.Invoke();
        }

        static void IncrementTypeCounts(LogType t, int delta)
        {
            // Match Unity-ish buckets:
            // Info: Log + Assert
            // Warn: Warning
            // Error: Error + Exception
            switch (t)
            {
                case LogType.Warning:
                    s_warnCount += delta;
                    break;
                case LogType.Error:
                case LogType.Exception:
                    s_errorCount += delta;
                    break;
                default:
                    s_infoCount += delta;
                    break;
            }
        }

        static void RecomputeCounts()
        {
            int i = 0, w = 0, e = 0;
            for (var k = 0; k < s_entries.Count; k++)
            {
                var le = s_entries[k];
                var c = Mathf.Max(1, le.count);
                switch (le.type)
                {
                    case LogType.Warning: w += c; break;
                    case LogType.Error:
                    case LogType.Exception: e += c; break;
                    default: i += c; break;
                }
            }

            s_infoCount = i;
            s_warnCount = w;
            s_errorCount = e;
        }

        static void FlushAndSave()
        {
            FlushOnUpdate();
            SaveToSession();
        }

        static void SaveToSession()
        {
            if (!s_dirty) return;

            try
            {
                var dump = new LogDump
                {
                    entries = s_entries,
                    version = s_version
                };
                SessionState.SetString(SessionKey, JsonUtility.ToJson(dump));
                s_dirty = false;
            }
            catch
            {
                // Silent by design: logging here risks recursion + spam
            }
        }

        static void LoadFromSession()
        {
            try
            {
                var json = SessionState.GetString(SessionKey, "");
                if (string.IsNullOrEmpty(json))
                {
                    s_entries.Clear();
                    s_version = 0;
                    s_dirty = false;
                    s_infoCount = s_warnCount = s_errorCount = 0;
                    return;
                }

                var dump = JsonUtility.FromJson<LogDump>(json);
                if (dump?.entries == null)
                {
                    s_entries.Clear();
                    s_version = 0;
                    s_dirty = false;
                    s_infoCount = s_warnCount = s_errorCount = 0;
                    return;
                }

                s_entries.Clear();
                s_entries.AddRange(dump.entries);
                s_version = dump.version;
                s_dirty = false;

                FixupEntries(s_entries);

                RecomputeCounts();
            }
            catch
            {
                s_entries.Clear();
                s_version = 0;
                s_dirty = false;
                s_infoCount = s_warnCount = s_errorCount = 0;
            }
        }

        internal static void ExportTo(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                var dump = new LogDump
                {
                    entries = new List<LogEntry>(s_entries),
                    version = s_version
                };
                File.WriteAllText(path, JsonUtility.ToJson(dump, true));
            }
            catch (Exception e)
            {
                DLog.LogW($"DLogConsole: Export failed: {e.Message}");
            }
        }

        internal static void ImportFrom(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            try
            {
                var json = File.ReadAllText(path);
                var dump = JsonUtility.FromJson<LogDump>(json);
                if (dump?.entries == null)
                    return;

                s_entries.Clear();
                s_entries.AddRange(dump.entries);
                FixupEntries(s_entries);
                RecomputeCounts();

                s_version++;
                s_dirty = true;
                Changed?.Invoke();
            }
            catch (Exception e)
            {
                DLog.LogW($"DLogConsole: Import failed: {e.Message}");
            }
        }

        static void FixupEntries(IList<LogEntry> entries)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (string.IsNullOrEmpty(e.richMessage))
                    e.richMessage = s_linkLike.Replace(e.message ?? "", "<color=#00FFFF><b>$0</b></color>");
                if (string.IsNullOrEmpty(e.file) || e.line <= 0)
                    TryExtractFileLine(e.message, e.stackTrace, out e.file, out e.line);
                if (e.count <= 0) e.count = 1;
            }
        }

        internal static bool TryExtractFileLine(string message, string stack, out string file, out int line)
        {
            file = null;
            line = 0;

            // 1) [file:123]
            var m = s_bracketFileLine.Match(message ?? "");
            if (m.Success)
            {
                file = m.Groups[1].Value;
                int.TryParse(m.Groups[2].Value, out line);
                return line > 0;
            }

            // 2) compiler format: [COMPILATION] file(123): ...
            m = s_compilationFileLine.Match(message ?? "");
            if (m.Success)
            {
                file = m.Groups[1].Value;
                int.TryParse(m.Groups[2].Value, out line);
                return line > 0;
            }

            // 3) stack trace: " in C:\path\file.cs:line 123"
            if (!string.IsNullOrEmpty(stack))
            {
                var lines = stack.Split('\n');
                for (var i = 0; i < lines.Length; i++)
                {
                    var s = lines[i];
                    m = s_stackInLine.Match(s);
                    if (m.Success)
                    {
                        file = m.Groups[1].Value;
                        int.TryParse(m.Groups[2].Value, out line);
                        return line > 0;
                    }

                    m = s_stackLooseFileLine.Match(s);
                    if (m.Success)
                    {
                        file = m.Groups[1].Value;
                        int.TryParse(m.Groups[2].Value, out line);
                        return line > 0;
                    }
                }
            }

            return false;
        }
    }
}