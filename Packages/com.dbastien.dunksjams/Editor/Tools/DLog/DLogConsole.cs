// Assets/Editor/DLog/DLogConsole.cs

using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

[InitializeOnLoad]
public sealed partial class DLogConsole : EditorWindow, IHasCustomMenu
{
    private const string TimeFormat = "HH:mm:ss.fff";
    private const int MaxStackLinesVisible = 10;
    private const float MaxStackHeight = 260f;

    static DLogConsole()
    {
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool("DLogConsole.StartupShown", false))
                return;

            SessionState.SetBool("DLogConsole.StartupShown", true);
            ShowWindow();
        };
    }

    [MenuItem("‽/DLog/Window")]
    public static void ShowWindow()
    {
        Type consoleType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
        var window = GetWindow<DLogConsole>("DLog", true, consoleType);
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

    private readonly List<LogEntry> _view = new(256);
    private int _lastDrawnVersion = -1;
    private int _expandedCount;
    private readonly HashSet<LogEntry> _selectedEntries = new();
    private int _lastSelectedIndex = -1;

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
        if (_rowStyle != null &&
            _stackStyle != null &&
            _stackLineStyle != null &&
            _iconInfoSml != null &&
            _tooltipStyle != null &&
            _timeStyle != null)
            if (_lastProSkin == EditorGUIUtility.isProSkin)
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
        Color timeColor = EditorGUIUtility.isProSkin ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.25f, 0.25f, 0.25f);
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
        _iconErrSml = IconContentSafe("console.erroricon.sml", "console.erroricon");
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
        Texture icon = EditorGUIUtility.IconContent(iconName).image;
        if (icon == null)
            icon = EditorGUIUtility.IconContent("UnityEditor.ConsoleWindow").image;

        titleContent = new GUIContent("DLog", icon);
    }

    private static GUIContent IconContentSafe(string primary, string fallback)
    {
        GUIContent c = EditorGUIUtility.IconContent(primary);
        if (c == null || c.image == null)
            c = EditorGUIUtility.IconContent(fallback);
        return c ?? new GUIContent();
    }

    private static GUIStyle TryFindStyle(params string[] names)
    {
        if (names == null || names.Length == 0) return null;
        for (var i = 0; i < names.Length; ++i)
        {
            GUIStyle style = GUI.skin.FindStyle(names[i]);
            if (style != null) return style;
        }

        return null;
    }

    public void AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(EditorGUIUtility.TrTextContent("Open Player Log"), false,
            UnityEditorInternal.InternalEditorUtility.OpenPlayerConsole);
        menu.AddItem(EditorGUIUtility.TrTextContent("Open Editor Log"), false,
            UnityEditorInternal.InternalEditorUtility.OpenEditorConsole);
        menu.AddSeparator("");
        menu.AddItem(EditorGUIUtility.TrTextContent("Export DLog..."), false, ExportLogToFile);
        menu.AddItem(EditorGUIUtility.TrTextContent("Import DLog..."), false, ImportLogFromFile);
        menu.AddSeparator("");
        menu.AddItem(EditorGUIUtility.TrTextContent("Show Timestamp"), _showTime, ToggleShowTime);
        menu.AddSeparator("");
        AddStackTraceLoggingMenu(menu);
    }

    private void ToggleShowTime() => _showTime = !_showTime;

    private void ExportLogToFile()
    {
        string path = EditorUtility.SaveFilePanel("Export DLog", "", "DLog.json", "json");
        if (string.IsNullOrEmpty(path))
            return;

        DLogHub.ExportTo(path);
    }

    private void ImportLogFromFile()
    {
        string path = EditorUtility.OpenFilePanel("Import DLog", "", "json");
        if (string.IsNullOrEmpty(path))
            return;

        DLogHub.ImportFrom(path);
        RebuildView();
        Repaint();
    }

    private struct StackTraceLogTypeData
    {
        public LogType logType;
        public StackTraceLogType stackTraceLogType;
    }

    private void ToggleLogStackTraces(object userData)
    {
        var data = (StackTraceLogTypeData)userData;
        PlayerSettings.SetStackTraceLogType(data.logType, data.stackTraceLogType);
    }

    private void ToggleLogStackTracesForAll(object userData)
    {
        foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            PlayerSettings.SetStackTraceLogType(logType, (StackTraceLogType)userData);
    }

    private void AddStackTraceLoggingMenu(GenericMenu menu)
    {
        foreach (LogType logType in Enum.GetValues(typeof(LogType)))
        foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
        {
            StackTraceLogTypeData data;
            data.logType = logType;
            data.stackTraceLogType = stackTraceLogType;

            menu.AddItem(
                EditorGUIUtility.TrTextContent($"Stack Trace Logging/{logType}/{stackTraceLogType}"),
                PlayerSettings.GetStackTraceLogType(logType) == stackTraceLogType,
                ToggleLogStackTraces,
                data);
        }

        var stackTraceLogTypeForAll = (int)PlayerSettings.GetStackTraceLogType(LogType.Log);
        foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            if (PlayerSettings.GetStackTraceLogType(logType) != (StackTraceLogType)stackTraceLogTypeForAll)
            {
                stackTraceLogTypeForAll = -1;
                break;
            }

        foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
            menu.AddItem(
                EditorGUIUtility.TrTextContent($"Stack Trace Logging/All/{stackTraceLogType}"),
                (StackTraceLogType)stackTraceLogTypeForAll == stackTraceLogType,
                ToggleLogStackTracesForAll,
                stackTraceLogType);
    }

    private static void AppendEntryText(StringBuilder sb, LogEntry e, bool includeStack)
    {
        if (e == null)
            return;

        string message = e.message ?? "";
        string stack = e.stackTrace ?? "";

        if (includeStack && !string.IsNullOrEmpty(stack))
        {
            if (!string.IsNullOrEmpty(message))
                sb.Append(message).Append("\n\n").Append(stack);
            else
                sb.Append(stack);
        }
        else
        {
            if (!string.IsNullOrEmpty(message))
                sb.Append(message);
            else if (!string.IsNullOrEmpty(stack))
                sb.Append(stack);
        }
    }

    private void CopySelectionToClipboard(bool includeStack)
    {
        if (_selectedEntries.Count == 0)
            return;

        var sb = new StringBuilder();
        for (var i = 0; i < _view.Count; i++)
        {
            LogEntry e = _view[i];
            if (!_selectedEntries.Contains(e))
                continue;

            if (sb.Length > 0)
                sb.Append("\n\n");

            AppendEntryText(sb, e, includeStack);
        }

        GUIUtility.systemCopyBuffer = sb.ToString();
    }

    private void HandleCopyShortcut()
    {
        Event evt = Event.current;
        if (evt.type != EventType.KeyDown)
            return;
        if (EditorGUIUtility.editingTextField)
            return;
        if (!evt.control && !evt.command)
            return;
        if (evt.keyCode != KeyCode.C)
            return;
        if (_selectedEntries.Count == 0)
            return;

        CopySelectionToClipboard(evt.shift);
        evt.Use();
    }

    private void OnGUI()
    {
        if (!EnsureGuiReady())
            return;

        _hoverTooltip = null;
        if (Event.current.type == EventType.MouseMove)
            Repaint();

        HandleCopyShortcut();

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

        bool newData = _lastDrawnVersion != DLogHub.Version;

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_view.Count > 0)
        {
            if (_expandedCount == 0)
                DrawRowsVirtualized();
            else
                for (var i = 0; i < _view.Count; i++)
                    DrawRow(_view[i], i);
        }

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
        if (_toolbarInfo == null) _toolbarInfo = new GUIContent();
        if (_toolbarWarn == null) _toolbarWarn = new GUIContent();
        if (_toolbarErr == null) _toolbarErr = new GUIContent();

        _toolbarInfo.text = DLogHub.InfoCount.ToString();
        _toolbarInfo.image = _iconInfoSml.image;
        _toolbarInfo.tooltip = "Info";

        _toolbarWarn.text = DLogHub.WarningCount.ToString();
        _toolbarWarn.image = _iconWarnSml.image;
        _toolbarWarn.tooltip = "Warnings";

        _toolbarErr.text = DLogHub.ErrorCount.ToString();
        _toolbarErr.image = _iconErrSml.image;
        _toolbarErr.tooltip = "Errors";

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
            {
                DLogHub.Clear();
                GUIUtility.keyboardControl = 0;
            }

            var clearDropContent = new GUIContent("v");
            Rect clearDropRect =
                GUILayoutUtility.GetRect(clearDropContent, EditorStyles.toolbarDropDown, GUILayout.Width(18f));
            if (GUI.Button(clearDropRect, clearDropContent, EditorStyles.toolbarDropDown))
                ShowClearMenu(clearDropRect);

            _autoScroll = GUILayout.Toggle(_autoScroll, "Auto Scroll", EditorStyles.toolbarButton);
            _showTime = GUILayout.Toggle(_showTime, "Time", EditorStyles.toolbarButton);

            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope(GUIStyle.none))
            {
                GUI.SetNextControlName("DLogSearchField");
                GUIStyle searchStyle = TryFindStyle("ToolbarSearchTextField", "ToolbarSeachTextField") ??
                                       EditorStyles.toolbarTextField;
                _filter = GUILayout.TextField(_filter ?? "", searchStyle, GUILayout.Width(240));
                Rect searchRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(searchRect, MouseCursor.Text);

                bool hasFilter = !string.IsNullOrEmpty(_filter);
                GUIStyle cancelStyle = TryFindStyle("ToolbarSearchCancelButton") ?? GUIStyle.none;
                GUIStyle cancelEmptyStyle = TryFindStyle("ToolbarSearchCancelButtonEmpty") ?? GUIStyle.none;
                if (GUILayout.Button(GUIContent.none, hasFilter ? cancelStyle : cancelEmptyStyle))
                {
                    _filter = "";
                    GUI.FocusControl(null);
                }
            }

            GUILayout.FlexibleSpace();

            // Console-like type toggles with icon + count
            _showInfo = GUILayout.Toggle(_showInfo, _toolbarInfo, EditorStyles.toolbarButton, GUILayout.MinWidth(52));
            _showWarnings = GUILayout.Toggle(_showWarnings, _toolbarWarn, EditorStyles.toolbarButton,
                GUILayout.MinWidth(52));
            _showErrors = GUILayout.Toggle(_showErrors, _toolbarErr, EditorStyles.toolbarButton,
                GUILayout.MinWidth(52));

            GUILayout.Space(10);
            _maxShown = EditorGUILayout.IntSlider(_maxShown, 50, 2000, GUILayout.Width(260));
        }
    }

    private void ShowClearMenu(Rect rect)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Clear on Play"), DLogHub.ClearOnPlay,
            () => DLogHub.ClearOnPlay = !DLogHub.ClearOnPlay);
        menu.AddItem(new GUIContent("Clear on Build"), DLogHub.ClearOnBuild,
            () => DLogHub.ClearOnBuild = !DLogHub.ClearOnBuild);
        menu.AddItem(new GUIContent("Clear on Recompile"), DLogHub.ClearOnRecompile,
            () => DLogHub.ClearOnRecompile = !DLogHub.ClearOnRecompile);
        menu.DropDown(rect);
    }

    private void RebuildView()
    {
        _cachedVersion = DLogHub.Version;
        _cachedFilter = _filter;
        _cachedInfo = _showInfo;
        _cachedWarn = _showWarnings;
        _cachedErr = _showErrors;

        _view.Clear();
        _expandedCount = 0;

        IReadOnlyList<LogEntry> entries = DLogHub.Entries;
        if (entries == null || entries.Count == 0)
            return;

        var added = 0;
        for (int i = entries.Count - 1; i >= 0 && added < _maxShown; i--)
        {
            LogEntry e = entries[i];
            if (!PassesFilters(e))
                continue;

            _view.Add(e);
            if (e.expanded && !string.IsNullOrEmpty(e.stackTrace))
                _expandedCount++;
            added++;
        }

        _view.Reverse();

        if (_selectedEntries.Count > 0)
        {
            var viewSet = new HashSet<LogEntry>(_view);
            _selectedEntries.RemoveWhere(e => !viewSet.Contains(e));
            if (_selectedEntries.Count == 0)
                _lastSelectedIndex = -1;
            else if (_lastSelectedIndex < 0 ||
                     _lastSelectedIndex >= _view.Count ||
                     !_selectedEntries.Contains(_view[_lastSelectedIndex]))
                _lastSelectedIndex = _view.FindIndex(e => _selectedEntries.Contains(e));
        }
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
            string f = _filter;
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
}