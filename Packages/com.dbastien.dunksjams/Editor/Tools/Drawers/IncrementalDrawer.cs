using UnityEditor;
using UnityEngine;

public abstract class IncrementalDrawer<T> : PropertyDrawer
{
    public static readonly GUIContent iconToolbarPlus = EditorGUIUtils.IconContentSafe("d_Toolbar Plus", "Toolbar Plus", "Increase");
    public static readonly GUIContent iconToolbarMinus = EditorGUIUtils.IconContentSafe("d_Toolbar Minus", "Toolbar Minus", "Decrease");

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        const float buttonWidth = 24f;

        EditorGUI.BeginProperty(rect, label, prop);

        float fieldWidth = rect.width - buttonWidth * 2;

        var propRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
        var plusRect = new Rect(propRect.x + propRect.width, rect.y, buttonWidth, rect.height);
        var minusRect = new Rect(plusRect.x + plusRect.width, rect.y, buttonWidth, rect.height);

        EditorGUI.PropertyField(propRect, prop, label);

        T increment = GetIncrement();

        if (GUI.Button(minusRect, iconToolbarMinus)) DecrementValue(prop, increment);
        if (GUI.Button(plusRect, iconToolbarPlus)) IncrementValue(prop, increment);

        EditorGUI.EndProperty();
    }

    protected abstract T GetIncrement();
    protected abstract void IncrementValue(SerializedProperty prop, T inc);
    protected abstract void DecrementValue(SerializedProperty prop, T dec);
}

[CustomPropertyDrawer(typeof(FloatIncrementalAttribute))]
public class FloatIncrementalDrawer : IncrementalDrawer<float>
{
    protected override float GetIncrement() => ((FloatIncrementalAttribute)attribute).Increment;
    protected override void IncrementValue(SerializedProperty prop, float inc) => prop.floatValue += inc;
    protected override void DecrementValue(SerializedProperty prop, float dec) => prop.floatValue -= dec;
}

[CustomPropertyDrawer(typeof(IntIncrementalAttribute))]
public class IntIncrementalDrawer : IncrementalDrawer<int>
{
    protected override int GetIncrement() => ((IntIncrementalAttribute)attribute).Increment;
    protected override void IncrementValue(SerializedProperty prop, int inc) => prop.intValue += inc;
    protected override void DecrementValue(SerializedProperty prop, int dec) => prop.intValue -= dec;
}
