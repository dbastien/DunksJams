// Assets/Editor/DLog/DLogConsole.Rendering.cs
using System;
using UnityEditor;
using UnityEngine;

public sealed partial class DLogConsole
{
    private static float GetRowHeight() => EditorGUIUtility.singleLineHeight + 6f;

    private static float GetSkipHeight(int rowCount, float rowHeight, float spacing)
    {
        if (rowCount <= 0)
            return 0f;

        return rowHeight * rowCount + spacing * Mathf.Max(0, rowCount - 1);
    }

    private void DrawRowsVirtualized()
    {
        int count = _view.Count;
        if (count == 0)
            return;

        float rowHeight = GetRowHeight();
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        float rowStride = rowHeight + spacing;

        float scrollY = _scroll.y;
        if (float.IsNaN(scrollY) || float.IsInfinity(scrollY))
            scrollY = float.MaxValue;

        int firstIndex = Mathf.Clamp(Mathf.FloorToInt(scrollY / rowStride), 0, count - 1);
        int visibleCount = Mathf.CeilToInt(position.height / rowStride) + 4;
        int lastIndex = Mathf.Min(count - 1, firstIndex + visibleCount);

        float before = GetSkipHeight(firstIndex, rowHeight, spacing);
        if (before > 0f)
            GUILayout.Space(before);

        for (int i = firstIndex; i <= lastIndex; i++)
            DrawRow(_view[i], i);

        int afterCount = count - lastIndex - 1;
        float after = GetSkipHeight(afterCount, rowHeight, spacing);
        if (after > 0f)
            GUILayout.Space(after);
    }

    private void DrawRow(LogEntry e, int index)
    {
        float rowH = GetRowHeight();
        Rect row = EditorGUILayout.GetControlRect(false, rowH);

        // Background (subtle)
        var bg = EditorGUIUtility.isProSkin
            ? new Color(0.16f, 0.16f, 0.16f)
            : new Color(0.90f, 0.90f, 0.90f);
        EditorGUI.DrawRect(row, bg);

        bool isSelected = _selectedEntries.Contains(e);
        if (isSelected)
        {
            var selected = EditorGUIUtility.isProSkin
                ? new Color(0.24f, 0.48f, 0.90f, 0.35f)
                : new Color(0.24f, 0.48f, 0.90f, 0.20f);
            EditorGUI.DrawRect(row, selected);
        }

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
            bool wasExpanded = e.expanded;
            if (GUI.Button(arrowRect, e.expanded ? "▼" : "▶", EditorStyles.miniButton))
                e.expanded = !e.expanded;

            if (e.expanded != wasExpanded)
                _expandedCount += e.expanded ? 1 : -1;
        }

        var ev = Event.current;
        if (ev.type == EventType.MouseDown && ev.button == 0 && row.Contains(ev.mousePosition))
        {
            bool hasShift = ev.shift;
            bool hasCtrl = ev.control || ev.command;

            if (hasShift && _lastSelectedIndex >= 0 && _lastSelectedIndex < _view.Count)
            {
                int start = Mathf.Min(_lastSelectedIndex, index);
                int end = Mathf.Max(_lastSelectedIndex, index);

                if (!hasCtrl)
                    _selectedEntries.Clear();

                for (int i = start; i <= end; i++)
                    _selectedEntries.Add(_view[i]);
            }
            else if (hasCtrl)
            {
                if (!_selectedEntries.Add(e))
                    _selectedEntries.Remove(e);
            }
            else
            {
                _selectedEntries.Clear();
                _selectedEntries.Add(e);
            }

            _lastSelectedIndex = index;
            Repaint();
        }

        // Context click
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
        EnsureMessageCache(e);
        if (e == null || string.IsNullOrEmpty(e.plainMessage) || !e.messageLink.hasLink)
            return;

        string visible = e.plainMessage;
        string file = e.messageLink.file;
        int lineNumber = e.messageLink.lineNumber;
        int linkStart = e.messageLink.start;
        int linkLength = e.messageLink.length;

        if (!string.IsNullOrEmpty(e.file))
        {
            bool hasEntryName = TryGetFileName(e.file, out string entryName);
            bool hasMatchName = TryGetFileName(file, out string matchName);
            if (hasEntryName && (!hasMatchName || string.Equals(entryName, matchName, StringComparison.OrdinalIgnoreCase)))
                file = e.file;
        }

        Rect linkRect = GetLinkRect(msgRect, visible, linkStart, linkLength, _rowStyle);
        if (linkRect.width <= 0f)
            return;

        EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
        SetHoverTooltip(linkRect, file, lineNumber);

        var evt = Event.current;
        bool hasModifier = evt.shift || evt.control || evt.command || evt.alt;
        if (!hasModifier && evt.type == EventType.MouseDown && evt.button == 0 && linkRect.Contains(evt.mousePosition))
        {
            OpenFileAtLine(file, lineNumber);
            evt.Use();
        }
    }

    private void DrawStackTrace(LogEntry e)
    {
        EnsureStackCache(e);
        if (e == null || e.stackLineInfos == null || e.stackLineInfos.Length == 0)
            return;

        float lineHeight = EditorGUIUtility.singleLineHeight + 2f;
        float height = e.stackScrollHeight > 0f
            ? e.stackScrollHeight
            : Mathf.Min(MaxStackHeight, lineHeight * Math.Min(e.stackLineInfos.Length, MaxStackLinesVisible));

        bool isCompilation = e.message != null && e.message.StartsWith("[COMPILATION]", StringComparison.Ordinal);
        SetStyleTextColor(_stackLineStyle, isCompilation ? StackTextColor() : TintFor(e.type));

        var oldBg = GUI.backgroundColor;
        GUI.backgroundColor = EditorGUIUtility.isProSkin
            ? new Color(0.12f, 0.12f, 0.12f)
            : new Color(0.82f, 0.82f, 0.82f);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = oldBg;
        e.stackScroll = EditorGUILayout.BeginScrollView(e.stackScroll, GUILayout.Height(height));

        var oldColor = GUI.contentColor;
        GUI.contentColor = Color.white;

        for (int i = 0; i < e.stackLineInfos.Length; ++i)
        {
            var info = e.stackLineInfos[i];
            Rect rect = EditorGUILayout.GetControlRect(false, lineHeight);
            GUI.Label(rect, info.displayLine ?? info.line ?? "", _stackLineStyle);

            Rect linkRect = default;
            if (info.link.hasLink)
            {
                linkRect = GetLinkRect(rect, info.line, info.link.start, info.link.length, _stackLineStyle);
                if (linkRect.width > 0f)
                {
                    EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
                    SetHoverTooltip(linkRect, info.link.file, info.link.lineNumber);
                }
            }

            var evt = Event.current;
            if (info.link.hasLink && linkRect.width > 0f &&
                evt.type == EventType.MouseDown && evt.button == 0 &&
                linkRect.Contains(evt.mousePosition))
            {
                OpenFileAtLine(info.link.file, info.link.lineNumber);
                evt.Use();
            }
        }

        GUI.contentColor = oldColor;

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
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
        if (DLogHub.TryExtractFileLine(e.message, e.stackTrace, out var file, out var line))
            OpenFileAtLine(file, line);
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
