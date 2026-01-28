// Assets/Editor/DLogConsole.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

public sealed class DLogConsole : EditorWindow
{
    private const string TimeFormat = "HH:mm:ss.fff";

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

        [NonSerialized] public Vector2 stackScroll;
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

                string msg = BuildCompilationMessage(m, out string file, out int line, out string richMessage);

                var e = MakeEntry(msg, stackTrace: "", lt);
                e.file = file;
                e.line = line;
                e.richMessage = richMessage;

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

        private static string BuildCompilationMessage(CompilerMessage m, out string file, out int line, out string richMessage)
        {
            file = m.file ?? "";
            line = Mathf.Max(0, m.line);

            string description = StripCompilationPrefix(m.message);
            string displayFile = ToAssetPath(file);
            string lineSegment = line > 0 ? $"({line})" : "";
            string prefix = string.IsNullOrEmpty(displayFile)
                ? "[COMPILATION]"
                : $"[COMPILATION] {displayFile}{lineSegment}:";

            string msg = string.IsNullOrEmpty(description) ? prefix : $"{prefix} {description}";
            richMessage = ColorizePrefix(m.type, prefix, description);

            return msg;
        }

        private static string StripCompilationPrefix(string message)
        {
            if (string.IsNullOrEmpty(message)) return "";

            int idx = message.IndexOf("):", StringComparison.Ordinal);
            if (idx >= 0 && idx + 2 < message.Length)
            {
                int start = idx + 2;
                if (message[start] == ' ') start++;
                return message.Substring(start);
            }

            return message;
        }

        private static string ToAssetPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            string p = path.Replace("\\", "/");
            int idx = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
            return idx >= 0 ? p.Substring(idx) : p;
        }

        private static string ColorizePrefix(CompilerMessageType type, string prefix, string description)
        {
            string color = type == CompilerMessageType.Warning
                ? "#FFCC00"
                : "#FF5555";

            if (string.IsNullOrEmpty(prefix)) return description ?? "";
            if (string.IsNullOrEmpty(description)) return $"<color={color}>{prefix}</color>";
            return $"<color={color}>{prefix}</color> {description}";
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

    [MenuItem("Window/DLog")]
    public static void ShowWindow()
    {
        var window = GetWindow<DLogConsole>();
        window.UpdateTitle();
    }

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
    private GUIStyle _stackLineStyle;
    private GUIStyle _countBadgeStyle;
    private GUIStyle _timeStyle;
    private GUIStyle _tooltipStyle;
    private string _hoverTooltip;
    private bool _lastProSkin;
    private bool _showTime = true;
    private float _timeWidth;

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
        UpdateTitle();
        RebuildView();
    }

    private void OnDisable() => DLogHub.Changed -= OnHubChanged;

    private void OnHubChanged() => Repaint();

    private bool EnsureGuiReady()
    {
        if (_rowStyle != null && _stackStyle != null && _stackLineStyle != null && _iconInfoSml != null && _tooltipStyle != null && _timeStyle != null)
        {
            if (_lastProSkin == EditorGUIUtility.isProSkin)
                return true;
        }

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

    private static GUIStyle TryFindStyle(params string[] names)
    {
        if (names == null || names.Length == 0) return null;
        for (int i = 0; i < names.Length; ++i)
        {
            var style = GUI.skin.FindStyle(names[i]);
            if (style != null) return style;
        }
        return null;
    }

    private void BuildStylesAndIcons()
    {
        _lastProSkin = EditorGUIUtility.isProSkin;

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

        _stackLineStyle = new GUIStyle(EditorStyles.label)
        {
            richText = true,
            wordWrap = false,
            clipping = TextClipping.Clip
        };
        SetStyleTextColor(_stackLineStyle, StackTextColor());

        _tooltipStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            padding = new RectOffset(6, 6, 2, 2)
        };
        SetStyleTextColor(_tooltipStyle, StackTextColor());

        _timeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip
        };
        var timeColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.25f, 0.25f, 0.25f);
        SetStyleTextColor(_timeStyle, timeColor);
        _timeWidth = _timeStyle.CalcSize(new GUIContent("00:00:00.000")).x + 6f;

        _countBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };

        // Console-like icons
        _iconInfoSml = IconContentSafe("console.infoicon.sml", "console.infoicon");
        _iconWarnSml = IconContentSafe("console.warnicon.sml", "console.warnicon");
        _iconErrSml  = IconContentSafe("console.erroricon.sml", "console.erroricon");
        UpdateTitle();
    }

    private static void SetStyleTextColor(GUIStyle style, Color color)
    {
        if (style == null) return;
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
    }

    private static Color StackTextColor()
        => EditorGUIUtility.isProSkin ? new Color(0.84f, 0.84f, 0.84f) : new Color(0.1f, 0.1f, 0.1f);

    private void UpdateTitle()
    {
        string iconName = EditorGUIUtility.isProSkin ? "d_UnityEditor.ConsoleWindow" : "UnityEditor.ConsoleWindow";
        var icon = EditorGUIUtility.IconContent(iconName).image;
        if (icon == null)
            icon = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image;

        titleContent = new GUIContent("DLog", icon);
    }

    private void OnGUI()
    {
        if (!EnsureGuiReady())
            return;

        _hoverTooltip = null;
        if (Event.current.type == EventType.MouseMove)
            Repaint();

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

        DrawHoverTooltip();
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
            _showTime = GUILayout.Toggle(_showTime, "Time", EditorStyles.toolbarButton);

            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope(GUIStyle.none))
            {
                GUI.SetNextControlName("DLogSearchField");
                var searchStyle = TryFindStyle("ToolbarSearchTextField", "ToolbarSeachTextField") ?? EditorStyles.toolbarTextField;
                _filter = GUILayout.TextField(_filter ?? "", searchStyle, GUILayout.Width(240));
                var searchRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(searchRect, MouseCursor.Text);

                bool hasFilter = !string.IsNullOrEmpty(_filter);
                var cancelStyle = TryFindStyle("ToolbarSearchCancelButton") ?? GUIStyle.none;
                var cancelEmptyStyle = TryFindStyle("ToolbarSearchCancelButtonEmpty") ?? GUIStyle.none;
                if (GUILayout.Button(GUIContent.none, hasFilter ? cancelStyle : cancelEmptyStyle))
                {
                    _filter = "";
                    GUI.FocusControl(null);
                }
            }

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
        if (_showTime)
        {
            var timeRect = new Rect(msgX, row.y + 1f, _timeWidth, row.height - 2f);
            GUI.Label(timeRect, FormatTime(e.timeMsUtc), _timeStyle);
            msgX = timeRect.xMax + 4f;
        }

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

        bool isCompilation = e.message != null && e.message.StartsWith("[COMPILATION]", StringComparison.Ordinal);
        var oldColor = GUI.contentColor;
        if (!isCompilation)
            GUI.contentColor = TintFor(e.type);
        GUI.Label(msgRect, e.richMessage ?? e.message ?? "", _rowStyle);
        GUI.contentColor = oldColor;
        DrawMessageLink(e, msgRect);

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
            DrawStackTrace(e);
        }
    }

    private void DrawMessageLink(LogEntry e, Rect msgRect)
    {
        string raw = e.richMessage ?? e.message ?? "";
        if (string.IsNullOrEmpty(raw))
            return;

        string visible = StripRichText(raw);
        if (string.IsNullOrEmpty(visible))
            return;

        if (!TryGetMessageLinkSpan(visible, out string file, out int lineNumber, out int linkStart, out int linkLength))
            return;

        if (!string.IsNullOrEmpty(e.file))
        {
            string entryName = Path.GetFileName(e.file);
            string matchName = Path.GetFileName(file);
            if (string.IsNullOrEmpty(matchName) ||
                string.Equals(entryName, matchName, StringComparison.OrdinalIgnoreCase))
            {
                file = e.file;
            }
        }

        Rect linkRect = GetLinkRect(msgRect, visible, linkStart, linkLength, _rowStyle);
        if (linkRect.width <= 0f)
            return;

        EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
        SetHoverTooltip(linkRect, file, lineNumber);

        var evt = Event.current;
        if (evt.type == EventType.MouseDown && evt.button == 0 && linkRect.Contains(evt.mousePosition))
        {
            OpenFileAtLine(file, lineNumber);
            evt.Use();
        }
    }

    private static readonly Regex s_stackAtFileLine = new Regex(@"\(at\s+(.+?):(\d+)\)", RegexOptions.Compiled);
    private static readonly Regex s_stackInLine = new Regex(@"\s+in\s+(.+):line\s+(\d+)\s*$", RegexOptions.Compiled);
    private static readonly Regex s_stackLooseFileLine = new Regex(@"(.+?\.\w+):(\d+)", RegexOptions.Compiled);
    private static readonly Regex s_bracketFileLine = new Regex(@"\[(.+?):(\d+)\]", RegexOptions.Compiled);
    private static readonly Regex s_richTextTags = new Regex(@"</?(color|a|b)(\s+[^>]*)?>", RegexOptions.Compiled);

    private void DrawStackTrace(LogEntry e)
    {
        string[] lines = e.stackTrace.Split('\n');
        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        float height = Mathf.Min(260f, lineHeight * Math.Min(lines.Length, 10));

        bool isCompilation = e.message != null && e.message.StartsWith("[COMPILATION]", StringComparison.Ordinal);
        SetStyleTextColor(_stackLineStyle, isCompilation ? StackTextColor() : TintFor(e.type));

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        e.stackScroll = EditorGUILayout.BeginScrollView(e.stackScroll, GUILayout.Height(height));

        var oldColor = GUI.contentColor;
        GUI.contentColor = Color.white;

        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(line)) continue;

            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            bool hasLink = TryGetStackLinkSpan(line, out string file, out int lineNumber, out int linkStart, out int linkLength);
            string displayLine = hasLink ? WrapHighlight(line, linkStart, linkLength) : line;
            GUI.Label(rect, displayLine, _stackLineStyle);

            Rect linkRect = default;
            if (hasLink)
            {
                linkRect = GetLinkRect(rect, line, linkStart, linkLength, _stackLineStyle);
                if (linkRect.width > 0f)
                {
                    EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
                    SetHoverTooltip(linkRect, file, lineNumber);
                }
            }

            var evt = Event.current;
            if (hasLink && linkRect.width > 0f &&
                evt.type == EventType.MouseDown && evt.button == 0 &&
                linkRect.Contains(evt.mousePosition))
            {
                OpenFileAtLine(file, lineNumber);
                evt.Use();
            }
        }

        GUI.contentColor = oldColor;

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private static bool TryParseStackLine(string line, out string file, out int lineNumber)
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

    private static string ToAssetPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        string p = path.Replace("\\", "/");
        int idx = p.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase);
        return idx >= 0 ? p.Substring(idx) : p;
    }

    private static string s_projectRoot;
    private static string ProjectRoot
    {
        get
        {
            if (!string.IsNullOrEmpty(s_projectRoot)) return s_projectRoot;

            string dataPath = Application.dataPath.Replace("\\", "/");
            if (dataPath.EndsWith("/Assets", StringComparison.OrdinalIgnoreCase))
                s_projectRoot = dataPath.Substring(0, dataPath.Length - "/Assets".Length);
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

        var match = s_bracketFileLine.Match(line);
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

        var match = s_stackAtFileLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackInLine.Match(line);
        if (TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length))
            return true;

        match = s_stackLooseFileLine.Match(line);
        return TryBuildLinkSpan(match, line, out file, out lineNumber, out start, out length);
    }

    private bool TryBuildLinkSpan(Match match, string line, out string file, out int lineNumber, out int start, out int length)
    {
        file = null;
        lineNumber = 0;
        start = 0;
        length = 0;

        if (match == null || !match.Success || match.Groups.Count < 3)
            return false;

        file = match.Groups[1].Value;
        if (!int.TryParse(match.Groups[2].Value, out lineNumber) || lineNumber <= 0)
            return false;

        string fileName = Path.GetFileName(file);
        if (string.IsNullOrEmpty(fileName))
            return false;

        int pathStart = match.Groups[1].Index;
        int offset = file.Replace("\\", "/").LastIndexOf('/');
        if (offset >= 0) offset += 1;
        else
        {
            offset = file.LastIndexOf('\\');
            offset = offset >= 0 ? offset + 1 : 0;
        }

        start = pathStart + offset;
        string token = $"{fileName}:{lineNumber}";
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

    private static string StripRichText(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return s_richTextTags.Replace(text, "");
    }

    private static string FormatTime(long timeMsUtc)
    {
        if (timeMsUtc <= 0) return "--:--:--.---";
        return DateTimeOffset.FromUnixTimeMilliseconds(timeMsUtc)
            .ToLocalTime()
            .ToString(TimeFormat);
    }

    private static string WrapHighlight(string source, int start, int length)
    {
        string color = EditorGUIUtility.isProSkin ? "#8FD6FF" : "#005A9E";
        return source.Substring(0, start)
               + $"<color={color}><b>"
               + source.Substring(start, length)
               + "</b></color>"
               + source.Substring(start + length);
    }

    private Rect GetLinkRect(Rect lineRect, string line, int start, int length, GUIStyle style)
    {
        if (style == null) return default;
        if (start < 0 || length <= 0 || start + length > line.Length) return default;

        string prefix = line.Substring(0, start);
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

    private void DrawHoverTooltip()
    {
        if (string.IsNullOrEmpty(_hoverTooltip) || _tooltipStyle == null)
            return;

        float pad = 6f;
        float height = EditorGUIUtility.singleLineHeight + 4f;
        var rect = new Rect(pad, position.height - height - pad, position.width - pad * 2f, height);
        var bg = EditorGUIUtility.isProSkin
            ? new Color(0.12f, 0.12f, 0.12f, 0.92f)
            : new Color(0.97f, 0.97f, 0.97f, 0.95f);

        EditorGUI.DrawRect(rect, bg);
        GUI.Label(rect, _hoverTooltip, _tooltipStyle);
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

