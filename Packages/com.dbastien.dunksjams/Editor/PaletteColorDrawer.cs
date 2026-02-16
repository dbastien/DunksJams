#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PaletteColorAttribute))]
public class PaletteColorDrawer : PropertyDrawer
{
    const float SwatchSize = 16f;
    const float Spacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Only operate on Color fields
        if (property.propertyType != SerializedPropertyType.Color)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        var attr = attribute as PaletteColorAttribute;

        // Main color field area
        var fieldRect = position;
        fieldRect.width -= (SwatchSize + Spacing) * 3f; // leave space for swatch bar + gear + advanced picker

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PropertyField(fieldRect, property, label);

        // Resolve palette for this drawer (explicit path on attribute takes precedence)
        var palettes = PaletteDatabase.Palettes;
        ColorPalette pal = null;
        if (!string.IsNullOrEmpty(attr.palettePath))
            pal = AssetDatabase.LoadAssetAtPath<ColorPalette>(attr.palettePath);
        if (pal == null && palettes.Count > 0) pal = palettes[0];

        // Swatch bar
        var swatchRect = new Rect(fieldRect.xMax + Spacing, position.y, SwatchSize, SwatchSize);
        DrawSwatchBar(swatchRect, property, pal);

        // Gear button - select and ping the palette asset so designer can inspect/edit it
        var gearRect = new Rect(swatchRect.xMax + Spacing, position.y, SwatchSize, SwatchSize);
        if (GUI.Button(gearRect, EditorGUIUtility.IconContent("d_FilterByLabel"), EditorStyles.iconButton))
        {
            if (pal != null)
            {
                Selection.activeObject = pal;
                EditorGUIUtility.PingObject(pal);
            }
        }

        // Advanced Picker button
        var pickerRect = new Rect(gearRect.xMax + Spacing, position.y, SwatchSize, SwatchSize);
        if (GUI.Button(pickerRect, EditorGUIUtility.IconContent("d_EyeDropper"), EditorStyles.iconButton))
        {
            DunksColorPickerWindow.ShowWindow(c =>
            {
                property.colorValue = c;
                property.serializedObject.ApplyModifiedProperties();
            }, property.colorValue);
        }

        EditorGUI.EndProperty();
    }

    void DrawSwatchBar(Rect rect, SerializedProperty property, ColorPalette pal)
    {
        if (pal == null)
        {
            EditorGUI.DrawRect(rect, Color.clear);
            return;
        }

        var colors = pal.ToArray();
        if (colors == null || colors.Length == 0)
        {
            EditorGUI.DrawRect(rect, Color.clear);
            return;
        }

        // Expand rect to show N swatches horizontally (clamped by rect width)
        var count = colors.Length;
        var sw = rect.width / count;
        var x = rect.x;
        for (var i = 0; i < count; ++i)
        {
            var r = new Rect(x + i * sw, rect.y, sw, rect.height);
            EditorGUI.DrawRect(r, colors[i]);
            var id = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                property.colorValue = colors[i];
                property.serializedObject.ApplyModifiedProperties();
                Event.current.Use();
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
}
#endif