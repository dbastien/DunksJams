#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

//todo: we have some gui scope stuff somewhere else, double check.


    static class FoldoutUi
    {
        static GUIStyle _headerStyle;
        static GUIStyle _subHeaderStyle;

        public const float HeaderHeight = 20f;
        public const float SubHeaderHeight = 18f;

        public static void DrawSplitter(bool boxed = false)
        {
            var r = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            r = FullWidth(r);

            var c = EditorGUIUtility.isProSkin
                ? new Color(0.10f, 0.10f, 0.10f, boxed ? 0.85f : 1f)
                : new Color(0.60f, 0.60f, 0.60f, boxed ? 0.65f : 0.8f);

            EditorGUI.DrawRect(r, c);
        }

        public static bool DrawHeaderFoldout(string title, bool state, bool subHeader = false, bool boxed = false)
        {
            var content = EditorGUIUtility.TrTempContent(title ?? string.Empty);
            var h = subHeader ? SubHeaderHeight : HeaderHeight;

            var row = GUILayoutUtility.GetRect(1f, h, GUILayout.ExpandWidth(true));

            var bg = FullWidth(row);
            DrawHeaderBackground(bg, boxed, subHeader);

            var indented = EditorGUI.IndentedRect(row);
            indented.yMin += 1f;

            var style = subHeader ? SubHeaderStyle : HeaderStyle;
            return EditorGUI.Foldout(indented, state, content, true, style);
        }

        static void DrawHeaderBackground(Rect rect, bool boxed, bool subHeader)
        {
            var baseTint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
            var alpha = subHeader ? 0.12f : 0.18f;
            if (boxed) alpha *= 0.85f;

            EditorGUI.DrawRect(rect, new Color(baseTint, baseTint, baseTint, alpha));

            var line = rect;
            line.yMin = line.yMax - 1f;
            line.height = 1f;

            var lc = EditorGUIUtility.isProSkin
                ? new Color(0f, 0f, 0f, 0.35f)
                : new Color(0f, 0f, 0f, 0.12f);

            EditorGUI.DrawRect(line, lc);
        }

        static Rect FullWidth(Rect r)
        {
            r.xMin = 0f;
            r.width += 4f;
            return r;
        }

        static GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle != null) return _headerStyle;

                var baseStyle =
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

        static GUIStyle SubHeaderStyle
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
    }

    /// <summary>Compact header foldout scope. Collapsed: full-width row header. Expanded: vertical "tab" on the left, content to the right (within a horizontal group).</summary>
    public readonly struct CompactSectionHeaderFoldoutScope : IDisposable
    {
        public static readonly GUIStyle NoStretchStyle = new() { stretchWidth = false };
        public const float Height = 17f;

        readonly EditorStoredValue<bool> _foldoutState;
        readonly bool _spaceAtStart;
        readonly bool _spaceAtEnd;

        // Track what we opened THIS event based on start-of-event state to avoid Begin/End mismatches when toggling.
        readonly bool _openedHorizontal;

        public bool IsExpanded => _foldoutState.Value;

        public CompactSectionHeaderFoldoutScope(
            string title,
            EditorStoredValue<bool> foldoutState,
            bool spaceAtStart = true,
            bool spaceAtEnd = true,
            bool boxed = false)
        {
            _foldoutState = foldoutState ?? throw new ArgumentNullException(nameof(foldoutState));
            _spaceAtStart = spaceAtStart;
            _spaceAtEnd = spaceAtEnd;

            FoldoutUi.DrawSplitter(boxed);

            using var changedScope = new GuiChangedScope();

            var wasExpanded = _foldoutState.Value;
            _openedHorizontal = wasExpanded;

            var titleContent = EditorGUIUtility.TrTempContent(title ?? string.Empty);

            var newState = wasExpanded
                ? DrawExpandedHeader(titleContent, wasExpanded, boxed)
                : DrawCollapsedHeader(titleContent, wasExpanded, boxed);

            _foldoutState.Value = newState;

            GUILayout.BeginVertical(boxed ? EditorStyles.helpBox : GUIStyle.none);

            if (_spaceAtStart)
                EditorGUILayout.Space();
        }

        static bool DrawCollapsedHeader(GUIContent title, bool state, bool boxed)
        {
            var rowRect = GUILayoutUtility.GetRect(1f, Height, GUILayout.ExpandWidth(true));

            var bg = rowRect;
            bg.xMin = 0f;
            bg.width += 4f;

            var tint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
            var alpha = boxed ? 0.14f : 0.18f;
            EditorGUI.DrawRect(bg, new Color(tint, tint, tint, alpha));

            var indented = EditorGUI.IndentedRect(rowRect);

            var foldoutRect = indented;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var labelRect = indented;
            labelRect.xMin = foldoutRect.xMax + 2f;
            labelRect.xMax -= 20f;

            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            return GUI.Toggle(foldoutRect, state, GUIContent.none, EditorStyles.foldout);
        }

        static bool DrawExpandedHeader(GUIContent title, bool state, bool boxed)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(NoStretchStyle, GUILayout.ExpandHeight(true));

            var titleSize = EditorStyles.boldLabel.CalcSize(title);
            var desiredHeight = titleSize.x + 18f + 4f;

            var layoutRect = GUILayoutUtility.GetRect(Height, desiredHeight, NoStretchStyle, GUILayout.ExpandHeight(true));

            var rotatedRect = new Rect(
                layoutRect.x,
                layoutRect.y + layoutRect.height,
                layoutRect.height,
                layoutRect.width);

            var titleRect = rotatedRect;
            titleRect.x += 18f;
            var toggleRect = rotatedRect;
            toggleRect.x += 2f;

            var oldMatrix = GUI.matrix;
            try
            {
                EditorGUIUtility.RotateAroundPivot(-90f, rotatedRect.position);

                var tint = EditorGUIUtility.isProSkin ? 0.12f : 1f;
                var alpha = boxed ? 0.14f : 0.18f;
                EditorGUI.DrawRect(rotatedRect, new Color(tint, tint, tint, alpha));

                EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);
                state = GUI.Toggle(toggleRect, state, GUIContent.none, EditorStyles.foldout);
            }
            finally
            {
                GUI.matrix = oldMatrix;
            }

            GUILayout.EndVertical();
            return state;
        }

        public void Dispose()
        {
            if (IsExpanded && _spaceAtEnd &&
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
        readonly EditorStoredValue<bool> _foldoutState;
        readonly bool _spaceAtEnd;

        public bool IsExpanded => _foldoutState.Value;

        public SectionHeaderFoldoutScope(
            string title,
            EditorStoredValue<bool> foldoutState,
            bool spaceAtStart = true,
            bool spaceAtEnd = true,
            bool subHeader = false,
            bool boxed = false)
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
            if (IsExpanded && _spaceAtEnd &&
                (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout))
                EditorGUILayout.Space();

            GUILayout.EndVertical();
        }
    }

    /// <summary>Convenience base for a compact foldout section.</summary>
    public abstract class CompactFoldoutSection
    {
        readonly string _title;
        readonly EditorStoredValue<bool> _state;
        readonly bool _boxed;

        protected CompactFoldoutSection(string title, EditorStoredValue<bool> state, bool boxed = false)
        {
            _title = title ?? string.Empty;
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _boxed = boxed;
        }

        public void Draw(bool spaceAtStart = true, bool spaceAtEnd = true)
        {
            if (!IsVisible) return;

            using var header = new CompactSectionHeaderFoldoutScope(_title, _state, spaceAtStart, spaceAtEnd, _boxed);
            if (header.IsExpanded)
                DrawContent();
        }

        public abstract bool IsVisible { get; }
        protected abstract void DrawContent();
    }
# endif