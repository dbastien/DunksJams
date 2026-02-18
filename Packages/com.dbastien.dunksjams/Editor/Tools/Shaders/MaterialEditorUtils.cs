using System;
using UnityEditor;
using UnityEngine;

public static class MaterialEditorUtils
{
    public static Rect TextureColorToggleInline
    (
        MaterialEditor matEditor, GUIContent label,
        MaterialProperty texProp, MaterialProperty colorToggleProp, MaterialProperty colorProp
    ) =>
        matEditor.TexturePropertySingleLine(label,
            texProp,
            colorToggleProp.floatValue != 0f ? colorProp : null,
            colorToggleProp);

    public static Rect TextureAutoSTInline
    (
        MaterialEditor editor, GUIContent label, MaterialProperty texProp,
        MaterialProperty scaleOffsetProp
    )
    {
        Rect lineRect = ShaderGUIUtils.GetControlRectForSingleLine();
        editor.TexturePropertyMiniThumbnail(lineRect, texProp, label.text, label.tooltip);
        SetSTKeywords(editor, texProp, scaleOffsetProp);
        return lineRect;
    }

    public static Rect TextureColorToggleAutoSTInline
    (
        MaterialEditor editor, GUIContent label,
        MaterialProperty texProp, MaterialProperty colorToggleProp, MaterialProperty colorProp,
        MaterialProperty scaleOffsetProp
    )
    {
        Rect rect = TextureColorToggleInline(editor, label, texProp, colorToggleProp, colorProp);
        SetSTKeywords(editor, texProp, scaleOffsetProp);
        return rect;
    }

    public static void STVector4Prop(MaterialEditor editor, GUIContent label, MaterialProperty scaleOffsetProp)
    {
        EditorGUI.showMixedValue = scaleOffsetProp.hasMixedValue;
        //EditorGUI.BeginChangeCheck();

        Vector4 scaleOffsetVec = scaleOffsetProp.vectorValue;

        var texScale = new Vector2(scaleOffsetVec.x, scaleOffsetVec.y);
        texScale = EditorGUILayout.Vector2Field(Styles.scale, texScale, Array.Empty<GUILayoutOption>());

        var texOffset = new Vector2(scaleOffsetVec.z, scaleOffsetVec.w);
        texOffset = EditorGUILayout.Vector2Field(Styles.offset, texOffset, Array.Empty<GUILayoutOption>());

        //if (EditorGUI.EndChangeCheck())
        {
            scaleOffsetProp.vectorValue = new Vector4(texScale.x, texScale.y, texOffset.x, texOffset.y);
        }
        EditorGUI.showMixedValue = false;
    }

    public static void SetSTKeywords(MaterialEditor editor, MaterialProperty texProp, MaterialProperty scaleOffsetProp)
    {
        Vector4 vec = scaleOffsetProp.vectorValue;
        var mat = editor.target as Material;
        mat.SetKeyword($"{texProp.name}_SCALE_ON", vec.x != 1.0f || vec.y != 1.0f);
        mat.SetKeyword($"{texProp.name}_OFFSET_ON", vec.z != 0.0f || vec.w != 0.0f);
    }

    private static class Styles
    {
        public static readonly GUIContent scale = new("Tiling",
            "Scale of texture - multiplied by texture coordinates from vertices");

        public static readonly GUIContent offset = new("Offset",
            "Offset of texture - added to texture coordinates from vertices");
    }
}