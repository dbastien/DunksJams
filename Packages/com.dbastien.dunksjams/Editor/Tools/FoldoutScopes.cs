#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

//todo: we have some gui scope stuff somewhere else, double check.

internal static class FoldoutUi
{
    public const float HeaderHeight = 20f;
    public const float SubHeaderHeight = 18f;
    private static GUIStyle _headerStyle;
    private static GUIStyle _subHeaderStyle;

    private static GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle != null) return _headerStyle;

            GUIStyle baseStyle =
                GUI.skin.FindStyle("FoldoutHeader") ??
                GUI.skin.FindStyle("IN Foldout") ??
                new GUIStyle(EditorStyles.foldout);

            _headerStyle = new GUIStyle(baseStyle)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 0f,
                richText = false
            };

            _headerStyle.padding.left = Mathf.Max(_headerStyle.padding.left, 16);
            _headerStyle.padding.top = Mathf.Max(_headerStyle.padding.top, 2);
            _headerStyle.padding.bottom = Mathf.Max(_headerStyle.padding.bottom, 2);
            return _headerStyle;
        }
    }

    private static GUIStyle SubHeaderStyle
    {
        get
        {
            if (_subHeaderStyle != null) return _subHeaderStyle;
            _subHeaderStyle = new GUIStyle(HeaderStyle)
            {
                fontStyle = FontStyle.Bold
            };
            _subHeaderStyle.padding.top = 1;
            _subHeaderStyle.padding.bottom = 1;
            return _subHeaderStyle;
        }
    }

    public static void DrawSplitter(bool boxed = false)
    {
        Rect r = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
        r = FullWidth(r);

        Color c = EditorGUIUtility.isProSkin
            ? new Color(0.10f, 0.10f, 0.10f, boxed ? 0.85f : 1f)
            : new Color(0.60f, 0.60f, 0.60f, boxed ? 0.65f : 0.8f);

        EditorGUI.DrawRect(r, c);
    }

    public static bool DrawHeaderFoldout(string title, bool state, bool subHeader = false, bool boxed = false)
    {
        GUIContent content = EditorGUIUtility.TrTempContent(title ?? string.Empty);
        float h = subHeader ? SubHeaderHeight : HeaderHeight;

        Rect row = GUILayoutUtility.GetRect(1f, h, GUILayout.ExpandWidth(true));

        Rect bg = FullWidth(row);
        DrawHeaderBackground(bg, boxed, subHeader);

        Rect indented = EditorGUI.IndentedRect(row);
        indented.yMin += 1f;

        GUIStyle style = subHeader ? SubHeaderStyle : HeaderStyle;
        return EditorGUI.Foldout(indented, state, content, true, style);
    }

    private static void DrawHeaderBackground(Rect rect, bool boxed, bool subHeader)
    {
        float baseTint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
        float alpha = subHeader ? 0.12f : 0.18f;
        if (boxed) alpha *= 0.85f;

        EditorGUI.DrawRect(rect, new Color(baseTint, baseTint, baseTint, alpha));

        Rect line = rect;
        line.yMin = line.yMax - 1f;
        line.height = 1f;

        Color lc = EditorGUIUtility.isProSkin
            ? new Color(0f, 0f, 0f, 0.35f)
            : new Color(0f, 0f, 0f, 0.12f);

        EditorGUI.DrawRect(line, lc);
    }

    private static Rect FullWidth(Rect r)
    {
        r.xMin = 0f;
        r.width += 4f;
        return r;
    }
}

/// <summary>
///     Compact header foldout scope. Collapsed: full-width row header. Expanded: vertical "tab" on the left, content
///     to the right (within a horizontal group).
/// </summary>
public readonly struct CompactSectionHeaderFoldoutScope : IDisposable
{
    public static readonly GUIStyle NoStretchStyle = new() { stretchWidth = false };
    public const float Height = 17f;

    private readonly EditorStoredValue<bool> _foldoutState;
    private readonly bool _spaceAtStart;
    private readonly bool _spaceAtEnd;

    // Track what we opened THIS event based on start-of-event state to avoid Begin/End mismatches when toggling.
    private readonly bool _openedHorizontal;

    public bool IsExpanded => _foldoutState.Value;

    public CompactSectionHeaderFoldoutScope
    (
        string title,
        EditorStoredValue<bool> foldoutState,
        bool spaceAtStart = true,
        bool spaceAtEnd = true,
        bool boxed = false
    )
    {
        _foldoutState = foldoutState ?? throw new ArgumentNullException(nameof(foldoutState));
        _spaceAtStart = spaceAtStart;
        _spaceAtEnd = spaceAtEnd;

        FoldoutUi.DrawSplitter(boxed);

        using var changedScope = new GuiChangedScope();

        bool wasExpanded = _foldoutState.Value;
        _openedHorizontal = wasExpanded;

        GUIContent titleContent = EditorGUIUtility.TrTempContent(title ?? string.Empty);

        bool newState = wasExpanded
            ? DrawExpandedHeader(titleContent, wasExpanded, boxed)
            : DrawCollapsedHeader(titleContent, wasExpanded, boxed);

        _foldoutState.Value = newState;

        GUILayout.BeginVertical(boxed ? EditorStyles.helpBox : GUIStyle.none);

        if (_spaceAtStart)
            EditorGUILayout.Space();
    }

    private static bool DrawCollapsedHeader(GUIContent title, bool state, bool boxed)
    {
        Rect rowRect = GUILayoutUtility.GetRect(1f, Height, GUILayout.ExpandWidth(true));

        Rect bg = rowRect;
        bg.xMin = 0f;
        bg.width += 4f;

        float tint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
        float alpha = boxed ? 0.14f : 0.18f;
        EditorGUI.DrawRect(bg, new Color(tint, tint, tint, alpha));

        Rect indented = EditorGUI.IndentedRect(rowRect);

        Rect foldoutRect = indented;
        foldoutRect.y += 1f;
        foldoutRect.width = 13f;
        foldoutRect.height = 13f;

        Rect labelRect = indented;
        labelRect.xMin = foldoutRect.xMax + 2f;
        labelRect.xMax -= 20f;

        EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
        return GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
    }

    private static bool DrawExpandedHeader(GUIContent title, bool state, bool boxed)
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(NoStretchStyle, GUILayout.ExpandHeight(true));

        Vector2 titleSize = EditorStyles.boldLabel.CalcSize(title);
        float desiredHeight = titleSize.x + 18f + 4f;

        Rect layoutRect = GUILayoutUtility.GetRect(Height, desiredHeight, NoStretchStyle, GUILayout.ExpandHeight(true));

        var rotatedRect = new Rect(
            layoutRect.x,
            layoutRect.y + layoutRect.height,
            layoutRect.height,
            layoutRect.width);

        Rect titleRect = rotatedRect;
        titleRect.x += 18f;
        Rect toggleRect = rotatedRect;
        toggleRect.x += 2f;

        Matrix4x4 oldMatrix = GUI.matrix;
        try
        {
            EditorGUIUtility.RotateAroundPivot(-90f, rotatedRect.position);

            float tint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
            float alpha = boxed ? 0.14f : 0.18f;
            EditorGUI.DrawRect(rotatedRect, new Color(tint, tint, tint, alpha));

            EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);
            state = GUI.Toggle(toggleRect, state, GUIContent.none, EditorStyles.foldout);
        }
        finally { GUI.matrix = oldMatrix; }

        GUILayout.EndVertical();
        return state;
    }

    public void Dispose()
    {
        if (IsExpanded &&
            _spaceAtEnd &&
            (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
            EditorGUILayout.Space();

        GUILayout.EndVertical();

        if (_openedHorizontal)
            GUILayout.EndHorizontal();
    }
}

/// <summary>Standard header foldout scope with a CoreEditorUtils-ish look, without SRP Core dependency.</summary>
public readonly struct SectionHeaderFoldoutScope : IDisposable
{
    private readonly EditorStoredValue<bool> _foldoutState;
    private readonly bool _spaceAtEnd;

    public bool IsExpanded => _foldoutState.Value;

    public SectionHeaderFoldoutScope
    (
        string title,
        EditorStoredValue<bool> foldoutState,
        bool spaceAtStart = true,
        bool spaceAtEnd = true,
        bool subHeader = false,
        bool boxed = false
    )
    {
        _foldoutState = foldoutState ?? throw new ArgumentNullException(nameof(foldoutState));
        _spaceAtEnd = spaceAtEnd;

        FoldoutUi.DrawSplitter(boxed);
        GUILayout.BeginVertical(boxed ? EditorStyles.helpBox : GUIStyle.none);

        using var changedScope = new GuiChangedScope();

        _foldoutState.Value = FoldoutUi.DrawHeaderFoldout(title, _foldoutState.Value, subHeader, boxed);

        if (spaceAtStart)
            EditorGUILayout.Space();
    }

    public void Dispose()
    {
        if (IsExpanded &&
            _spaceAtEnd &&
            (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
            EditorGUILayout.Space();

        GUILayout.EndVertical();
    }
}

/// <summary>Convenience base for a compact foldout section.</summary>
public abstract class CompactFoldoutSection
{
    private readonly bool _boxed;
    private readonly EditorStoredValue<bool> _state;
    private readonly string _title;

    protected CompactFoldoutSection(string title, EditorStoredValue<bool> state, bool boxed = false)
    {
        _title = title ?? string.Empty;
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _boxed = boxed;
    }

    public abstract bool IsVisible { get; }

    public void Draw(bool spaceAtStart = true, bool spaceAtEnd = true)
    {
        if (!IsVisible) return;

        using var header = new CompactSectionHeaderFoldoutScope(_title, _state, spaceAtStart, spaceAtEnd, _boxed);
        if (header.IsExpanded)
            DrawContent();
    }

    protected abstract void DrawContent();
}
# endif