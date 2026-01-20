using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        var rangeAttr = (MinMaxSliderAttribute)attribute;
        var vec = prop.vector2Value;
        EditorGUI.MinMaxSlider(pos, label, ref vec.x, ref vec.y, rangeAttr.Min, rangeAttr.Max);
        prop.vector2Value = vec;
    }
}