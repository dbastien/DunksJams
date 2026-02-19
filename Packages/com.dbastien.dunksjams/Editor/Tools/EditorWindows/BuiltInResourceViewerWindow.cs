using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuiltInResourceViewerWindow : EditorWindow
{
    [MenuItem("‽/Built-in Assets Viewer Window")]
    public static void ShowWindow() => GetWindow<BuiltInResourceViewerWindow>().Show();

    private struct Drawing
    {
        public Rect Rect;
        public Action Draw;
    }

    private readonly GUIContent inactiveText = new("inactive");
    private readonly GUIContent activeText = new("active");

    private const float ToolbarPad = 4f;
    private const float XPad = 32f;
    private const float XMin = 5f;
    private const float YMin = 5f;

    private List<Drawing> drawings;
    private List<Object> textures;

    private Vector2 scrollPos;
    private float contentHeight;

    private Rect lastWindowRect;
    private bool showingIcons = true;

    private void OnGUI()
    {
        DrawToolbar();

        var scrollArea = GetScrollAreaRect();

        EnsureLayout(scrollArea.width);

        DrawScrollArea(scrollArea);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        bool iconsPressed = GUILayout.Toggle(showingIcons, "Icons", EditorStyles.toolbarButton);
        bool stylesPressed = GUILayout.Toggle(!showingIcons, "Styles", EditorStyles.toolbarButton);

        bool nextShowingIcons = showingIcons;

        if (iconsPressed != showingIcons) nextShowingIcons = true;
        else if (stylesPressed == showingIcons) nextShowingIcons = false;

        if (nextShowingIcons != showingIcons)
        {
            showingIcons = nextShowingIcons;
            InvalidateLayout(resetScroll: true);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private Rect GetScrollAreaRect()
    {
        float top = GUILayoutUtility.GetLastRect().yMax + ToolbarPad;
        return new Rect(0f, top, position.width, position.height - top);
    }

    private void EnsureLayout(float availableWidth)
    {
        bool windowSizeChanged =
            !Mathf.Approximately(position.width, lastWindowRect.width) ||
            !Mathf.Approximately(position.height, lastWindowRect.height);

        if (windowSizeChanged && Event.current.type == EventType.Layout)
        {
            lastWindowRect = position;
            InvalidateLayout(resetScroll: false);
        }

        if (drawings != null) return;

        drawings = new List<Drawing>(2048);

        float contentWidth = Mathf.Max(0f, availableWidth);
        contentHeight = showingIcons ? BuildIcons(contentWidth) : BuildStyles(contentWidth);
    }

    private void InvalidateLayout(bool resetScroll)
    {
        drawings = null;

        if (resetScroll)
            scrollPos = Vector2.zero;
    }

    private void DrawScrollArea(Rect scrollArea)
    {
        if (scrollArea.width <= 0f || scrollArea.height <= 0f) return;

        float contentWidth = scrollArea.width - 1f;
        var contentRect = new Rect(0f, 0f, contentWidth, Mathf.Max(contentHeight, scrollArea.height));

        scrollPos = GUI.BeginScrollView(scrollArea, scrollPos, contentRect, false, true);

        float visibleTop = scrollPos.y;
        float visibleBottom = scrollPos.y + scrollArea.height;

        for (int i = 0; i < drawings.Count; i++)
        {
            var d = drawings[i];

            float y0 = d.Rect.y;
            float y1 = d.Rect.y + d.Rect.height;

            if (y1 < visibleTop || y0 > visibleBottom)
                continue;

            // IMPORTANT: use GUILayout.BeginArea so GUILayout calls inside d.Draw()
            // compute rects in the correct local layout context.
            GUILayout.BeginArea(d.Rect, GUI.skin.textField);
            d.Draw();
            GUILayout.EndArea();
        }

        GUI.EndScrollView();
    }

    private float BuildStyles(float availableWidth)
    {
        float x = XMin;
        float y = YMin;
        const float tileHeight = 60f;

        foreach (GUIStyle style in GUI.skin.customStyles)
        {
            GUIStyle thisStyle = style;

            float tileWidth = Mathf.Max(
                100f,
                GUI.skin.button.CalcSize(new GUIContent(thisStyle.name)).x,
                thisStyle.CalcSize(inactiveText, activeText).x
            ) + 16f;

            if (x + tileWidth > availableWidth - XPad && x > XMin)
            {
                x = XMin;
                y += tileHeight + 10f;
            }

            var rect = new Rect(x, y, tileWidth, tileHeight);
            float innerWidth = tileWidth - 8f;

            drawings.Add(new Drawing
            {
                Rect = rect,
                Draw = () =>
                {
                    GUILayout.BeginVertical();

                    if (GUILayout.Button(thisStyle.name, GUILayout.Width(innerWidth)))
                        CopyText("(GUIStyle)\"" + thisStyle.name + "\"");

                    GUILayout.BeginHorizontal();
                    GUILayout.Toggle(false, inactiveText, thisStyle, GUILayout.Width(innerWidth * 0.5f));
                    GUILayout.Toggle(false, activeText, thisStyle, GUILayout.Width(innerWidth * 0.5f));
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
            });

            x += tileWidth + 18f;
        }

        return y + tileHeight + YMin;
    }

    private float BuildIcons(float availableWidth)
    {
        float x = XMin;
        float y = YMin;

        EnsureTextureList();

        float rowHeight = 0f;

        foreach (Object obj in textures)
        {
            if (obj is not Texture2D texture) continue;
            if (string.IsNullOrEmpty(texture.name)) continue;

            Vector2 nameSize = GUI.skin.button.CalcSize(new GUIContent(texture.name));
            Vector2 texSize = new(texture.width, texture.height);

            float aspect = texSize.x / Mathf.Max(1f, texSize.y);
            if (aspect > 0.25f)
            {
                float scale = nameSize.x / Mathf.Max(1f, texSize.x);
                texSize *= scale;
            }

            float maxIconWidth = availableWidth - XPad - 8f;
            if (maxIconWidth > 0f && texSize.x > maxIconWidth)
            {
                float scale = maxIconWidth / Mathf.Max(1f, texSize.x);
                texSize *= scale;
            }

            float tileWidth = Mathf.Max(nameSize.x, texSize.x) + 8f;
            float tileHeight = texSize.y + nameSize.y + 8f;

            if (x + tileWidth > availableWidth - XPad && x > XMin)
            {
                x = XMin;
                y += rowHeight + 8f;
                rowHeight = 0f;
            }

            var rect = new Rect(x, y, tileWidth, tileHeight);
            float innerWidth = tileWidth - 8f;

            drawings.Add(new Drawing
            {
                Rect = rect,
                Draw = () =>
                {
                    GUILayout.BeginVertical();

                    if (GUILayout.Button(texture.name, GUILayout.Width(innerWidth)))
                        CopyText("EditorGUIUtility.FindTexture(\"" + texture.name + "\")");

                    // This MUST run under a proper GUILayout area/context (we do that in DrawScrollArea).
                    Rect texRect = GUILayoutUtility.GetRect(
                        texSize.x, texSize.x,
                        texSize.y, texSize.y,
                        GUILayout.ExpandHeight(false),
                        GUILayout.ExpandWidth(false)
                    );

                    EditorGUI.DrawTextureTransparent(texRect, texture);

                    GUILayout.EndVertical();
                }
            });

            rowHeight = Mathf.Max(rowHeight, tileHeight);
            x += tileWidth + 8f;
        }

        return y + rowHeight + YMin;
    }

    private void EnsureTextureList()
    {
        if (textures != null) return;

        textures = new List<Object>(Resources.FindObjectsOfTypeAll(typeof(Texture2D)));
        textures.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
    }

    private static void CopyText(string text)
    {
        var editor = new TextEditor { text = text };
        editor.SelectAll();
        editor.Copy();
    }
}