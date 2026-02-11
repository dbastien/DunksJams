using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuiltInResourceViewerWindow : EditorWindow
{
    [MenuItem("‽/Built-in Assets Viewer Window")]
    public static void ShowWindow()
    {
        var w = EditorWindow.GetWindow<BuiltInResourceViewerWindow>();
        w.Show();
    }

    private struct Drawing
    {
        public Rect Rect;
        public Action Draw;
    }

    private List<Drawing> drawings;

    private List<UnityEngine.Object> objects;
    private float scrollPos;
    private float maxY;
    private Rect oldPosition;

    private bool showingStyles = true;

    GUIContent inactiveText = new GUIContent("inactive");
    GUIContent activeText = new GUIContent("active");

    const float top = 36.0f;

    const float xPad = 32.0f;
    const float xMin = 5.0f;
    const float yMin = 5.0f;
    
    public void OnGUI()
    {
        if (this.position.width != this.oldPosition.width && Event.current.type == EventType.Layout)
        {
            this.drawings = null;
            this.oldPosition = this.position;
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Toggle(this.showingStyles, "Icons", EditorStyles.toolbarButton) != this.showingStyles)
        {
            this.showingStyles = !this.showingStyles;
            this.drawings = null;
        }

        if (GUILayout.Toggle(!this.showingStyles, "Styles", EditorStyles.toolbarButton) == this.showingStyles)
        {
            this.showingStyles = !showingStyles;
            this.drawings = null;
        }

        GUILayout.EndHorizontal();

        if (this.drawings == null)
        {
            this.drawings = new List<Drawing>();

            this.maxY = this.showingStyles ? ShowStyles() : ShowIcons();
        }

        Rect r = this.position;
        r.y = top;
        r.height -= r.y;
        r.x = r.width - 16.0f;
        r.width = 16.0f;

        var areaHeight = this.position.height - top;
        this.scrollPos = GUI.VerticalScrollbar(r, this.scrollPos, areaHeight, 0f, this.maxY);

        var area = new Rect(0, top, this.position.width - 16.0f, areaHeight);
        GUILayout.BeginArea(area);

        int count = 0;
        foreach (var draw in this.drawings)
        {
            Rect newRect = draw.Rect;
            newRect.y -= this.scrollPos;

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

    private float ShowStyles()
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

            if (x + width > this.position.width - xPad && x > xMin)
            {
                x = xMin;
                y += height + 10f;
            }

            draw.Rect = new Rect(x, y, width, height);

            width -= 8f;

            draw.Draw = () =>
            {
                if (GUILayout.Button(thisStyle.name, GUILayout.Width(width)))
                {
                    this.CopyText("(GUIStyle)\"" + thisStyle.name + "\"");
                }

                GUILayout.BeginHorizontal();
                GUILayout.Toggle(false, inactiveText, thisStyle, GUILayout.Width(width * 0.5f));
                GUILayout.Toggle(false, activeText, thisStyle, GUILayout.Width(width * 0.5f));
                GUILayout.EndHorizontal();
            };

            x += width + 18.0f;

            this.drawings.Add(draw);
        }

        return y;
    }

    private float ShowIcons()
    {
        var x = xMin;
        var y = yMin;

        if (this.objects == null)
        {
            this.objects = new List<UnityEngine.Object>(Resources.FindObjectsOfTypeAll(typeof(Texture2D)));
            this.objects.Sort((pA, pB) => String.Compare(pA.name, pB.name, StringComparison.OrdinalIgnoreCase));
        }

        var rowHeight = 0f;

        foreach (var oo in this.objects)
        {
            var texture = (Texture)oo;

            if (texture.name == string.Empty)
            {
                continue;
            }

            var draw = new Drawing();

            var textureNameSize = GUI.skin.button.CalcSize(new GUIContent(texture.name));
            var textureSize = new Vector2(texture.width, texture.height);

            //don't scale if very vertical
            float aspect = textureSize.x / textureSize.y;
            if (aspect > 0.25f)
            {
                float scale = textureNameSize.x / textureSize.x;
                textureSize *= scale;
            }

            var width = textureNameSize.x + 8f;
            var height = textureSize.y + textureNameSize.y + 8f;

            if (x + width > this.position.width - xPad)
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
                {
                    this.CopyText("EditorGUIUtility.FindTexture( \"" + texture.name + "\" )");
                }

                var textureRect = GUILayoutUtility.GetRect(
                    textureSize.x, textureSize.x, textureSize.y, textureSize.y, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
                EditorGUI.DrawTextureTransparent(textureRect, texture);
            };

            x += width + 8.0f;

            this.drawings.Add(draw);
        }

        return y;        
    }

    private void CopyText(string text)
    {
        var editor = new TextEditor()
        {
            text = text
        };

        editor.SelectAll();
        editor.Copy();
    }
}