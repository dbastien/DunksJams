using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuiltInResourceViewerWindow : EditorWindow
{
    [MenuItem("‽/Built-in Assets Viewer Window")]
    public static void ShowWindow()
    {
        var w = GetWindow<BuiltInResourceViewerWindow>();
        w.Show();
    }

    private struct Drawing
    {
        public Rect Rect;
        public Action Draw;
    }

    private List<Drawing> drawings;

    private List<Object> objects;
    private float scrollPos;
    private float maxY;
    private Rect oldPosition;

    private bool showingStyles = true;

    private GUIContent inactiveText = new("inactive");
    private GUIContent activeText = new("active");

    private const float toolbarPad = 4.0f;
    private const float scrollbarWidth = 16.0f;
    private const float xPad = 32.0f;
    private const float xMin = 5.0f;
    private const float yMin = 5.0f;

    public void OnGUI()
    {
        if (position.width != oldPosition.width && Event.current.type == EventType.Layout)
        {
            drawings = null;
            oldPosition = position;
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Toggle(showingStyles, "Icons", EditorStyles.toolbarButton) != showingStyles)
        {
            showingStyles = !showingStyles;
            drawings = null;
        }

        if (GUILayout.Toggle(!showingStyles, "Styles", EditorStyles.toolbarButton) == showingStyles)
        {
            showingStyles = !showingStyles;
            drawings = null;
        }

        GUILayout.EndHorizontal();

        float top = GUILayoutUtility.GetLastRect().yMax + toolbarPad;

        if (drawings == null)
        {
            drawings = new List<Drawing>();

            float contentWidth = position.width - scrollbarWidth;
            maxY = showingStyles ? ShowStyles(contentWidth) : ShowIcons(contentWidth);
        }

        Rect r = position;
        r.y = top;
        r.height -= r.y;
        r.x = r.width - scrollbarWidth;
        r.width = scrollbarWidth;

        float areaHeight = position.height - top;
        float scrollMax = Mathf.Max(maxY, areaHeight);
        scrollPos = GUI.VerticalScrollbar(r, scrollPos, areaHeight, 0f, scrollMax);
        scrollPos = Mathf.Clamp(scrollPos, 0f, Mathf.Max(0f, maxY - areaHeight));

        var area = new Rect(0, top, position.width - scrollbarWidth, areaHeight);
        GUILayout.BeginArea(area);

        var count = 0;
        foreach (Drawing draw in drawings)
        {
            Rect newRect = draw.Rect;
            newRect.y -= scrollPos;

            if (newRect.y + newRect.height > 0 && newRect.y < areaHeight)
            {
                GUILayout.BeginArea(newRect, GUI.skin.textField);
                draw.Draw();
                GUILayout.EndArea();

                ++count;
            }
        }

        GUILayout.EndArea();
    }

    private float ShowStyles(float availableWidth)
    {
        float x = xMin;
        float y = yMin;
        const float height = 60f;

        foreach (GUIStyle ss in GUI.skin.customStyles)
        {
            GUIStyle thisStyle = ss;

            var draw = new Drawing();

            float width = Mathf.Max(100f,
                              GUI.skin.button.CalcSize(new GUIContent(ss.name)).x,
                              ss.CalcSize(inactiveText, activeText).x) +
                          16f;

            if (x + width > availableWidth - xPad && x > xMin)
            {
                x = xMin;
                y += height + 10f;
            }

            draw.Rect = new Rect(x, y, width, height);

            width -= 8f;

            draw.Draw = () =>
            {
                if (GUILayout.Button(thisStyle.name, GUILayout.Width(width)))
                    CopyText("(GUIStyle)\"" + thisStyle.name + "\"");

                GUILayout.BeginHorizontal();
                GUILayout.Toggle(false, inactiveText, thisStyle, GUILayout.Width(width * 0.5f));
                GUILayout.Toggle(false, activeText, thisStyle, GUILayout.Width(width * 0.5f));
                GUILayout.EndHorizontal();
            };

            x += width + 18.0f;

            drawings.Add(draw);
        }

        return y + height + yMin;
    }

    private float ShowIcons(float availableWidth)
    {
        float x = xMin;
        float y = yMin;

        if (objects == null)
        {
            objects = new List<Object>(Resources.FindObjectsOfTypeAll(typeof(Texture2D)));
            objects.Sort((pA, pB) => string.Compare(pA.name, pB.name, StringComparison.OrdinalIgnoreCase));
        }

        var rowHeight = 0f;

        foreach (Object oo in objects)
        {
            var texture = (Texture)oo;

            if (texture.name == string.Empty) continue;

            var draw = new Drawing();

            Vector2 textureNameSize = GUI.skin.button.CalcSize(new GUIContent(texture.name));
            var textureSize = new Vector2(texture.width, texture.height);

            //don't scale if very vertical
            float aspect = textureSize.x / textureSize.y;
            if (aspect > 0.25f)
            {
                float scale = textureNameSize.x / textureSize.x;
                textureSize *= scale;
            }

            float maxIconWidth = availableWidth - xPad - 8f;
            if (maxIconWidth > 0f && textureSize.x > maxIconWidth)
            {
                float scale = maxIconWidth / textureSize.x;
                textureSize *= scale;
            }

            float width = Mathf.Max(textureNameSize.x, textureSize.x) + 8f;
            float height = textureSize.y + textureNameSize.y + 8f;

            if (x + width > availableWidth - xPad && x > xMin)
            {
                x = xMin;
                y += rowHeight + 8.0f;
                rowHeight = 0f;
            }

            draw.Rect = new Rect(x, y, width, height);

            rowHeight = Mathf.Max(rowHeight, height);

            width -= 8f;

            draw.Draw = () =>
            {
                if (GUILayout.Button(texture.name, GUILayout.Width(width)))
                    CopyText("EditorGUIUtility.FindTexture( \"" + texture.name + "\" )");

                Rect textureRect = GUILayoutUtility.GetRect(
                    textureSize.x, textureSize.x, textureSize.y, textureSize.y, GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(textureRect, texture);
            };

            x += width + 8.0f;

            drawings.Add(draw);
        }

        return y + rowHeight + yMin;
    }

    private void CopyText(string text)
    {
        var editor = new TextEditor
        {
            text = text
        };

        editor.SelectAll();
        editor.Copy();
    }
}