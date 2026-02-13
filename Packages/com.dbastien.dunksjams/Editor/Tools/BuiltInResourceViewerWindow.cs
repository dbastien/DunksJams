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

    const float top = 36.0f;

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

        GUILayout.BeginHorizontal();

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

        if (drawings == null)
        {
            drawings = new List<Drawing>();

            maxY = showingStyles ? ShowStyles() : ShowIcons();
        }

        var r = position;
        r.y = top;
        r.height -= r.y;
        r.x = r.width - 16.0f;
        r.width = 16.0f;

        var areaHeight = position.height - top;
        scrollPos = GUI.VerticalScrollbar(r, scrollPos, areaHeight, 0f, maxY);

        var area = new Rect(0, top, position.width - 16.0f, areaHeight);
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

    float ShowStyles()
    {
        var x = xMin;
        var y = yMin;

        foreach (var ss in GUI.skin.customStyles)
        {
            var thisStyle = ss;

            var draw = new Drawing();

            var width = Mathf.Max(100f,
                            GUI.skin.button.CalcSize(new GUIContent(ss.name)).x,
                            ss.CalcSize(inactiveText, activeText).x)
                        + 16f;

            const float height = 60f;

            if (x + width > position.width - xPad && x > xMin)
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

        return y;
    }

    float ShowIcons()
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

            var width = textureNameSize.x + 8f;
            var height = textureSize.y + textureNameSize.y + 8f;

            if (x + width > position.width - xPad)
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

        return y;
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