// Assets/Editor/DLogConsole.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public sealed class DLogConsole : EditorWindow
{
    [Serializable]
    private sealed class LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;

        public long timeMsUtc;   // survives serialization (no double)
        public int count = 1;    // collapsed repeat count (consecutive)

        public bool expanded;

        // Pre-formatted (rich text) to avoid regex each frame
        public string richMessage;

        // Optional "best guess" source location for fast double-click open
        public string file;
        public int line;
    }

    [Serializable]
    private sealed class LogDump
    {
        public List<LogEntry> entries = new List<LogEntry>();
        public int version;
    }

    [InitializeOnLoad]
    private static class DLogHub
    {
        private const int MaxEntries = 2000;
        private const string SessionKey = "DLogConsole.LogDump.v3";

        private static readonly ConcurrentQueue<LogEntry> s_pending = new ConcurrentQueue<LogEntry>();
        private static readonly List<LogEntry> s_entries = new List<LogEntry>(1024);

        private static int s_version;
        private static bool s_dirty;

        private static int s_infoCount;
        private static int s_warnCount;
        private static int s_errorCount;

        // If Unity prints compiler diagnostics to console, skip those (we capture from CompilationPipeline).
        private static readonly Regex s_compilerConsoleLine = new Regex(
            @"\(\d+,\d+\):\s*(warning|error)\s+CS\d+:",
            RegexOptions.Compiled);

        // Highlight patterns like [file:123] or file.cs:123 (best-effort)
        private static readonly Regex s_linkLike = new Regex(
            @"(\[[^\]]+:\d+\])|(\b[\w\./\\-]+\.\w+:\d+\b)",
            RegexOptions.Compiled);

        private static readonly Regex s_bracketFileLine = new Regex(@"\[(.+?):(\d+)\]", RegexOptions.Compiled);
        private static readonly Regex s_compilationFileLine = new Regex(@"^\[COMPILATION\]\s+(.+?)\((\d+)\):", RegexOptions.Compiled);
        private static readonly Regex s_stackInLine = new Regex(@"\s+in\s+(.+):line\s+(\d+)\s*$", RegexOptions.Compiled);
        private static readonly Regex s_stackLooseFileLine = new Regex(@"(.+?\.\w+):(\d+)", RegexOptions.Compiled);

        static DLogHub()
        {
            LoadFromSession();

            // Avoid accidental double-subscribe in weird Unity cycles:
            Application.logMessageReceivedThreaded -= OnLogThreaded;
            Application.logMessageReceivedThreaded += OnLogThreaded;

            CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

            EditorApplication.update -= FlushOnUpdate;
            EditorApplication.update += FlushOnUpdate;

            AssemblyReloadEvents.beforeAssemblyReload -= BeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;

            EditorApplication.quitting -= BeforeQuit;
            EditorApplication.quitting += BeforeQuit;
        }

        public static int Version => s_version;
        public static IReadOnlyList<LogEntry> Entries => s_entries;

        public static int InfoCount => s_infoCount;
        public static int WarningCount => s_warnCount;
        public static int ErrorCount => s_errorCount;

        public static event Action Changed;

        public static void Clear()
        {
            s_entries.Clear();
            while (s_pending.TryDequeue(out _)) { }

            s_infoCount = s_warnCount = s_errorCount = 0;

            s_version++;
            s_dirty = true;
            Changed?.Invoke();
        }

        private static void OnLogThreaded(string message, string stackTrace, LogType type)
        {
            // Skip compiler messages emitted to console if we capture via CompilationPipeline.
            if (!string.IsNullOrEmpty(message) && s_compilerConsoleLine.IsMatch(message))
                return;

            s_pending.Enqueue(MakeEntry(message, stackTrace, type));
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (messages == null || messages.Length == 0)
                return;

            // Only warnings/errors (info spam is not useful)
            for (int i = 0; i < messages.Length; i++)
            {
                var m = messages[i];
                if (m.type != CompilerMessageType.Warning && m.type != CompilerMessageType.Error)
                    continue;

                var lt = (m.type == CompilerMessageType.Warning) ? LogType.Warning : LogType.Error;

                string file = m.file ?? "";
                int line = Mathf.Max(0, m.line);

                string msg = string.IsNullOrEmpty(file)
                    ? $"[COMPILATION] {m.message}"
                    : $"[COMPILATION] {file}({line}): {m.message}";

                var e = MakeEntry(msg, stackTrace: "", lt);
                e.file = file;
                e.line = line;

                s_pending.Enqueue(e);
            }
        }

        private static LogEntry MakeEntry(string message, string stackTrace, LogType type)
        {
            var e = new LogEntry
            {
                message = message ?? "",
                stackTrace = stackTrace ?? "",
                type = type,
                timeMsUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            };

            // Pre-format once (rich text highlight)
            // Use RGB only to avoid alpha weirdness across versions.
            e.richMessage = s_linkLike.Replace(e.message, "<color=#00FFFF><b>$0</b></color>");

            // Try to pre-extract a location (best effort)
            TryExtractFileLine(e.message, e.stackTrace, out e.file, out e.line);

            return e;
        }

        private static void FlushOnUpdate()
        {
            if (s_pending.IsEmpty)
                return;

            bool changed = false;

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
                int remove = s_entries.Count - MaxEntries;
                s_entries.RemoveRange(0, remove);
                RecomputeCounts();
            }

            s_version++;
            s_dirty = true;

            Changed?.Invoke();
        }

        private static void IncrementTypeCounts(LogType t, int delta)
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

        private static void RecomputeCounts()
        {
            int i = 0, w = 0, e = 0;
            for (int k = 0; k < s_entries.Count; k++)
            {
                var le = s_entries[k];
                int c = Mathf.Max(1, le.count);
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

        private static void BeforeAssemblyReload()
        {
            FlushOnUpdate();
            SaveToSession();
        }

        private static void BeforeQuit()
        {
            FlushOnUpdate();
            SaveToSession();
        }

        private static void SaveToSession()
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

        private static void LoadFromSession()
        {
            try
            {
                string json = SessionState.GetString(SessionKey, "");
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

                // Fixup older sessions / missing computed fields
                for (int i = 0; i < s_entries.Count; i++)
                {
                    var e = s_entries[i];
                    if (string.IsNullOrEmpty(e.richMessage))
                        e.richMessage = s_linkLike.Replace(e.message ?? "", "<color=#00FFFF><b>$0</b></color>");
                    if (string.IsNullOrEmpty(e.file) || e.line <= 0)
                        TryExtractFileLine(e.message, e.stackTrace, out e.file, out e.line);
                    if (e.count <= 0) e.count = 1;
                }

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

        private static bool TryExtractFileLine(string message, string stack, out string file, out int line)
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
                for (int i = 0; i < lines.Length; i++)
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

    [MenuItem("Window/DLog Console")]
    public static void ShowWindow() => GetWindow<DLogConsole>("DLog Console");

    public static void ManualCheckCompilationWarnings()
    {
        if (EditorApplication.isCompiling)
        {
            DLog.Log("Compilation in progress; warnings will be captured automatically.");
            return;
        }

        DLog.Log("Requesting script compilation to collect warnings...");
        CompilationPipeline.RequestScriptCompilation();
    }

    public static void TestLogWarning() => DLog.LogW("DLog Console test warning.");

    private Vector2 _scroll;
    private bool _autoScroll = true;
    private string _filter = "";
    private bool _showInfo = true;
    private bool _showWarnings = true;
    private bool _showErrors = true;

    private int _maxShown = 200; // keep IMGUI smooth
    private int _cachedVersion = -1;
    private string _cachedFilter = null;
    private bool _cachedInfo, _cachedWarn, _cachedErr;

    private readonly List<LogEntry> _view = new List<LogEntry>(256);
    private int _lastDrawnVersion = -1;

    private GUIStyle _rowStyle;
    private GUIStyle _stackStyle;
    private GUIStyle _countBadgeStyle;

    private GUIContent _iconInfoSml;
    private GUIContent _iconWarnSml;
    private GUIContent _iconErrSml;

    private GUIContent _toolbarInfo;
    private GUIContent _toolbarWarn;
    private GUIContent _toolbarErr;

    private void OnEnable()
    {
        DLogHub.Changed -= OnHubChanged;
        DLogHub.Changed += OnHubChanged;

        // Unity 6: EditorStyles may not be initialized during domain reload.
        // Build styles lazily in OnGUI.
        RebuildView();
    }

    private void OnDisable() => DLogHub.Changed -= OnHubChanged;

    private void OnHubChanged() => Repaint();

    private bool EnsureGuiReady()
    {
        if (_rowStyle != null && _stackStyle != null && _iconInfoSml != null)
            return true;

        try
        {
            // Touch styles/icons to force init; these can be null during reload.
            _ = EditorStyles.label;
            _ = EditorStyles.toolbar;

            BuildStylesAndIcons();
            return true;
        }
        catch
        {
            EditorApplication.delayCall -= Repaint;
            EditorApplication.delayCall += Repaint;
            return false;
        }
    }

    private static GUIContent IconContentSafe(string primary, string fallback)
    {
        var c = EditorGUIUtility.IconContent(primary);
        if (c == null || c.image == null)
            c = EditorGUIUtility.IconContent(fallback);
        return c ?? new GUIContent();
    }

    private void BuildStylesAndIcons()
    {
        _rowStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };

        _stackStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = false
        };

        _countBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };

        // Console-like icons
        _iconInfoSml = IconContentSafe("console.infoicon.sml", "console.infoicon");
        _iconWarnSml = IconContentSafe("console.warnicon.sml", "console.warnicon");
        _iconErrSml  = IconContentSafe("console.erroricon.sml", "console.erroricon");
    }

    private void OnGUI()
    {
        if (!EnsureGuiReady())
            return;

        bool storeChanged = _cachedVersion != DLogHub.Version;
        bool filtersChanged =
            storeChanged ||
            _cachedFilter != _filter ||
            _cachedInfo != _showInfo ||
            _cachedWarn != _showWarnings ||
            _cachedErr != _showErrors;

        DrawToolbar();

        if (filtersChanged)
            RebuildView();

        bool newData = (_lastDrawnVersion != DLogHub.Version);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        for (int i = 0; i < _view.Count; i++)
            DrawRow(_view[i]);

        EditorGUILayout.EndScrollView();

        // Auto-scroll on new data (simple + reliable)
        if (Event.current.type == EventType.Repaint && newData && _autoScroll)
            _scroll.y = Mathf.Infinity;

        _lastDrawnVersion = DLogHub.Version;
    }

    private void DrawToolbar()
    {
        // Build toolbar button contents with current counts
        _toolbarInfo = new GUIContent(DLogHub.InfoCount.ToString(), _iconInfoSml.image, "Info");
        _toolbarWarn = new GUIContent(DLogHub.WarningCount.ToString(), _iconWarnSml.image, "Warnings");
        _toolbarErr  = new GUIContent(DLogHub.ErrorCount.ToString(), _iconErrSml.image, "Errors");

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
                DLogHub.Clear();

            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto Scroll", EditorStyles.toolbarButton);

            GUILayout.Space(10);
            _filter = GUILayout.TextField(_filter ?? "", EditorStyles.toolbarTextField, GUILayout.Width(240));

            GUILayout.FlexibleSpace();

            // Console-like type toggles with icon + count
            _showInfo = GUILayout.Toggle(_showInfo, _toolbarInfo, EditorStyles.toolbarButton, GUILayout.MinWidth(52));
            _showWarnings = GUILayout.Toggle(_showWarnings, _toolbarWarn, EditorStyles.toolbarButton, GUILayout.MinWidth(52));
            _showErrors = GUILayout.Toggle(_showErrors, _toolbarErr, EditorStyles.toolbarButton, GUILayout.MinWidth(52));

            GUILayout.Space(10);
            _maxShown = EditorGUILayout.IntSlider(_maxShown, 50, 2000, GUILayout.Width(260));
        }
    }

    private void RebuildView()
    {
        _cachedVersion = DLogHub.Version;
        _cachedFilter = _filter;
        _cachedInfo = _showInfo;
        _cachedWarn = _showWarnings;
        _cachedErr = _showErrors;

        _view.Clear();

        var entries = DLogHub.Entries;
        if (entries == null || entries.Count == 0)
            return;

        int added = 0;
        for (int i = entries.Count - 1; i >= 0 && added < _maxShown; i--)
        {
            var e = entries[i];
            if (!PassesFilters(e))
                continue;

            _view.Add(e);
            added++;
        }

        _view.Reverse();
    }

    private bool PassesFilters(LogEntry e)
    {
        switch (e.type)
        {
            case LogType.Warning:
                if (!_showWarnings) return false;
                break;

            case LogType.Error:
            case LogType.Exception:
                if (!_showErrors) return false;
                break;

            default:
                if (!_showInfo) return false;
                break;
        }

        if (!string.IsNullOrEmpty(_filter))
        {
            var f = _filter;
            if ((e.message?.IndexOf(f, StringComparison.OrdinalIgnoreCase) ?? -1) < 0 &&
                (e.stackTrace?.IndexOf(f, StringComparison.OrdinalIgnoreCase) ?? -1) < 0)
                return false;
        }

        return true;
    }

    private GUIContent IconFor(LogType t)
    {
        switch (t)
        {
            case LogType.Warning: return _iconWarnSml;
            case LogType.Error:
            case LogType.Exception: return _iconErrSml;
            default: return _iconInfoSml;
        }
    }

    private Color TintFor(LogType t)
    {
        switch (t)
        {
            case LogType.Warning: return new Color(1f, 0.8f, 0.1f);
            case LogType.Error:
            case LogType.Exception: return new Color(1f, 0.45f, 0.45f);
            default: return Color.white;
        }
    }

    private void DrawRow(LogEntry e)
    {
        float rowH = EditorGUIUtility.singleLineHeight + 6f;
        Rect row = EditorGUILayout.GetControlRect(false, rowH);

        // Background (subtle)
        var bg = EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.16f, 0.16f)
            : new Color(0.90f, 0.90f, 0.90f);
        EditorGUI.DrawRect(row, bg);

        // Layout regions
        Rect iconRect  = new Rect(row.x + 4f, row.y + 2f, 16f, 16f);
        Rect arrowRect = new Rect(row.x + 22f, row.y + 2f, 18f, row.height - 4f);

        // Count badge (right side, like Unity collapse count)
        Rect badgeRect = default;
        float badgeW = 0f;
        if (e.count > 1)
        {
            badgeW = 34f;
            badgeRect = new Rect(row.xMax - (badgeW + 4f), row.y + 2f, badgeW, row.height - 4f);
        }

        float msgX = arrowRect.xMax + 4f;
        float msgW = row.xMax - msgX - 6f - badgeW;
        Rect msgRect = new Rect(msgX, row.y + 1f, msgW, row.height - 2f);

        // Icon
        GUI.Label(iconRect, IconFor(e.type));

        // Expand button (only if stack trace exists)
        if (!string.IsNullOrEmpty(e.stackTrace))
        {
            if (GUI.Button(arrowRect, e.expanded ? "▼" : "▶", EditorStyles.miniButton))
                e.expanded = !e.expanded;
        }

        // Context click
        var ev = Event.current;
        if (ev.type == EventType.ContextClick && row.Contains(ev.mousePosition))
        {
            ShowContextMenu(e);
            ev.Use();
        }

        // Double click open
        if (ev.type == EventType.MouseDown && ev.button == 0 && ev.clickCount == 2 && row.Contains(ev.mousePosition))
        {
            OpenEntryLocation(e);
            ev.Use();
        }

        // Message
        if (string.IsNullOrEmpty(e.richMessage) && !string.IsNullOrEmpty(e.message))
        {
            // Just in case an old entry loaded without richMessage
            e.richMessage = e.message;
        }

        var oldColor = GUI.contentColor;
        GUI.contentColor = TintFor(e.type);
        GUI.Label(msgRect, e.richMessage ?? e.message ?? "", _rowStyle);
        GUI.contentColor = oldColor;

        // Count badge
        if (e.count > 1)
        {
            var badgeBg = EditorGUIUtility.isProSkin
                ? new Color(0.10f, 0.10f, 0.10f)
                : new Color(0.75f, 0.75f, 0.75f);

            EditorGUI.DrawRect(badgeRect, badgeBg);
            GUI.Label(badgeRect, e.count.ToString(), _countBadgeStyle);
        }

        // Stack trace block
        if (e.expanded && !string.IsNullOrEmpty(e.stackTrace))
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.TextArea(e.stackTrace, _stackStyle, GUILayout.Height(Mathf.Min(260, 18 * 10)));
            EditorGUILayout.EndVertical();
        }
    }

    private void ShowContextMenu(LogEntry e)
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("Copy Message"), false, () => GUIUtility.systemCopyBuffer = e.message ?? "");

        menu.AddItem(new GUIContent("Copy Full Entry"), false, () =>
        {
            string full = (e.message ?? "");
            if (!string.IsNullOrEmpty(e.stackTrace))
                full += "\n\n" + e.stackTrace;
            GUIUtility.systemCopyBuffer = full;
        });

        if (!string.IsNullOrEmpty(e.stackTrace))
            menu.AddItem(new GUIContent("Copy Stack Trace"), false, () => GUIUtility.systemCopyBuffer = e.stackTrace);

        menu.ShowAsContext();
    }

    private void OpenEntryLocation(LogEntry e)
    {
        if (!string.IsNullOrEmpty(e.file) && e.line > 0)
        {
            OpenFileAtLine(e.file, e.line);
            return;
        }

        // Fallback: try to extract quickly on demand
        if (TryExtractFileLine(e, out var file, out var line))
            OpenFileAtLine(file, line);
    }

    private bool TryExtractFileLine(LogEntry e, out string file, out int line)
    {
        file = e.file;
        line = e.line;
        if (!string.IsNullOrEmpty(file) && line > 0) return true;

        // [file:line]
        var m = Regex.Match(e.message ?? "", @"\[(.+?):(\d+)\]");
        if (m.Success)
        {
            file = m.Groups[1].Value;
            int.TryParse(m.Groups[2].Value, out line);
            return line > 0;
        }

        // stack: in ...:line N
        if (!string.IsNullOrEmpty(e.stackTrace))
        {
            foreach (var s in e.stackTrace.Split('\n'))
            {
                m = Regex.Match(s, @"\s+in\s+(.+):line\s+(\d+)\s*$");
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

    private void OpenFileAtLine(string filePath, int line)
    {
        string assetPath = null;

        if (!string.IsNullOrEmpty(filePath))
        {
            filePath = filePath.Replace('\\', '/');

            if (filePath.StartsWith("Assets/"))
            {
                assetPath = filePath;
            }
            else
            {
                // Best effort search by filename (only on user double-click)
                string fileName = System.IO.Path.GetFileName(filePath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    string[] guids = AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(fileName));
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (System.IO.Path.GetFileName(p) == fileName)
                        {
                            assetPath = p;
                            break;
                        }
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(assetPath))
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                AssetDatabase.OpenAsset(obj, line);
                return;
            }
        }

        // Absolute fallback
        if (!string.IsNullOrEmpty(filePath) &&
            System.IO.Path.IsPathRooted(filePath) &&
            System.IO.File.Exists(filePath))
        {
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, line);
            return;
        }

        DLog.LogW($"DLogConsole: Could not open '{filePath}' at line {line}");
    }
}

