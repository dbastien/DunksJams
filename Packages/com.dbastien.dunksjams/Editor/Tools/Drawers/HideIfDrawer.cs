using UnityEngine;
using UnityEditor;

public abstract class ConditionalPropertyDrawer : PropertyDrawer
{
    protected abstract bool ShouldShow(bool conditionValue);

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        if (EvaluateCondition(prop)) EditorGUI.PropertyField(rect, prop, label);
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) =>
        EvaluateCondition(prop) ? EditorGUI.GetPropertyHeight(prop, label) : -2f;

    bool EvaluateCondition(SerializedProperty prop)
    {
        var condition = prop.serializedObject.FindProperty(((ConditionalAttribute)attribute).ConditionField);
        return condition != null && ShouldShow(condition.boolValue);
    }
}

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfDrawer : ConditionalPropertyDrawer
{
    protected override bool ShouldShow(bool conditionValue) => !conditionValue;
}

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : ConditionalPropertyDrawer
{
    protected override bool ShouldShow(bool conditionValue) => conditionValue;
}