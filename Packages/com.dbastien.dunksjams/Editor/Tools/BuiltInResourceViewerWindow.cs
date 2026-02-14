using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuiltInResourceViewerWindow : EditorWindow
{
    [MenuItem("‽/Built-in Assets Viewer Window")]
    public static void ShowWindow()
    {
        var w = GetWindow<BuiltInResourceViewerWindow>();
        w.Show();
    }

    struct Drawing
    {
        public Rect Rect;
        public Action Draw;
    }

    List<Drawing> drawings;

    List<UnityEngine.Object> objects;
    float scrollPos;
    float maxY;
    Rect oldPosition;

    bool showingStyles = true;

    GUIContent inactiveText = new("inactive");
    GUIContent activeText = new("active");

    const float toolbarPad = 4.0f;
    const float scrollbarWidth = 16.0f;
    const float xPad = 32.0f;
    const float xMin = 5.0f;
    const float yMin = 5.0f;

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

        var top = GUILayoutUtility.GetLastRect().yMax + toolbarPad;

        if (drawings == null)
        {
            drawings = new List<Drawing>();

            var contentWidth = position.width - scrollbarWidth;
            maxY = showingStyles ? ShowStyles(contentWidth) : ShowIcons(contentWidth);
        }

        var r = position;
        r.y = top;
        r.height -= r.y;
        r.x = r.width - scrollbarWidth;
        r.width = scrollbarWidth;

        var areaHeight = position.height - top;
        var scrollMax = Mathf.Max(maxY, areaHeight);
        scrollPos = GUI.VerticalScrollbar(r, scrollPos, areaHeight, 0f, scrollMax);
        scrollPos = Mathf.Clamp(scrollPos, 0f, Mathf.Max(0f, maxY - areaHeight));

        var area = new Rect(0, top, position.width - scrollbarWidth, areaHeight);
        GUILayout.BeginArea(area);

        var count = 0;
        foreach (var draw in drawings)
        {
            var newRect = draw.Rect;
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

    float ShowStyles(float availableWidth)
    {
        var x = xMin;
        var y = yMin;
        const float height = 60f;

        foreach (var ss in GUI.skin.customStyles)
        {
            var thisStyle = ss;

            var draw = new Drawing();

            var width = Mathf.Max(100f,
                            GUI.skin.button.CalcSize(new GUIContent(ss.name)).x,
                            ss.CalcSize(inactiveText, activeText).x)
                        + 16f;

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

    float ShowIcons(float availableWidth)
    {
        var x = xMin;
        var y = yMin;

        if (objects == null)
        {
            objects = new List<UnityEngine.Object>(Resources.FindObjectsOfTypeAll(typeof(Texture2D)));
            objects.Sort((pA, pB) => string.Compare(pA.name, pB.name, StringComparison.OrdinalIgnoreCase));
        }

        var rowHeight = 0f;

        foreach (var oo in objects)
        {
            var texture = (Texture)oo;

            if (texture.name == string.Empty) continue;

            var draw = new Drawing();

            var textureNameSize = GUI.skin.button.CalcSize(new GUIContent(texture.name));
            var textureSize = new Vector2(texture.width, texture.height);

            //don't scale if very vertical
            var aspect = textureSize.x / textureSize.y;
            if (aspect > 0.25f)
            {
                var scale = textureNameSize.x / textureSize.x;
                textureSize *= scale;
            }

            var maxIconWidth = availableWidth - xPad - 8f;
            if (maxIconWidth > 0f && textureSize.x > maxIconWidth)
            {
                var scale = maxIconWidth / textureSize.x;
                textureSize *= scale;
            }

            var width = Mathf.Max(textureNameSize.x, textureSize.x) + 8f;
            var height = textureSize.y + textureNameSize.y + 8f;

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

                var textureRect = GUILayoutUtility.GetRect(
                    textureSize.x, textureSize.x, textureSize.y, textureSize.y, GUILayout.ExpandHeight(false),
                    GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(textureRect, texture);
            };

            x += width + 8.0f;

            drawings.Add(draw);
        }

        return y + rowHeight + yMin;
    }

    void CopyText(string text)
    {
        var editor = new TextEditor
        {
            text = text
        };

        editor.SelectAll();
        editor.Copy();
    }
}
