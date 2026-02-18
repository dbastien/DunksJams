using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyExtensions
{
    private static readonly Dictionary<SerializedPropertyType, Action<SerializedProperty>> _defaultSetters = new()
    {
        { SerializedPropertyType.Integer, prop => prop.intValue = 0 },
        { SerializedPropertyType.Boolean, prop => prop.boolValue = false },
        { SerializedPropertyType.Float, prop => prop.floatValue = 0f },
        { SerializedPropertyType.String, prop => prop.stringValue = "" },
        { SerializedPropertyType.ObjectReference, prop => prop.objectReferenceValue = null },
        { SerializedPropertyType.Enum, prop => prop.enumValueIndex = 0 },
        { SerializedPropertyType.Color, prop => prop.colorValue = Color.black },
        { SerializedPropertyType.LayerMask, prop => prop.intValue = 0 },
        { SerializedPropertyType.Vector2, prop => prop.vector2Value = Vector2.zero },
        { SerializedPropertyType.Vector3, prop => prop.vector3Value = Vector3.zero },
        { SerializedPropertyType.Vector4, prop => prop.vector4Value = Vector4.zero },
        { SerializedPropertyType.Rect, prop => prop.rectValue = Rect.zero },
        { SerializedPropertyType.ArraySize, prop => prop.intValue = 0 },
        { SerializedPropertyType.Character, prop => prop.intValue = 0 },
        { SerializedPropertyType.AnimationCurve, prop => prop.animationCurveValue = new AnimationCurve() },
        { SerializedPropertyType.Bounds, prop => prop.boundsValue = new Bounds(Vector3.zero, Vector3.zero) },
        { SerializedPropertyType.Gradient, prop => prop.gradientValue = new Gradient() },
        { SerializedPropertyType.Quaternion, prop => prop.quaternionValue = Quaternion.identity },
        { SerializedPropertyType.Vector2Int, prop => prop.vector2IntValue = Vector2Int.zero },
        { SerializedPropertyType.Vector3Int, prop => prop.vector3IntValue = Vector3Int.zero },
        { SerializedPropertyType.RectInt, prop => prop.rectIntValue = new RectInt() },
        { SerializedPropertyType.BoundsInt, prop => prop.boundsIntValue = new BoundsInt() }
    };

    public static void SetDefaultValue(this SerializedProperty property, bool logUnsupported = false)
    {
        Debug.Assert(_defaultSetters.TryGetValue(property.propertyType, out Action<SerializedProperty> setter));
        setter(property);
    }
}