using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ToggleHeaderAttribute))]
public class ToggleHeaderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var toggleHeader = (ToggleHeaderAttribute)attribute;
        SerializedProperty toggleProperty = property.serializedObject.FindProperty(toggleHeader.ToggleField);

        if (toggleProperty != null)
        {
            // Add spacing before the header to match Unity's header style
            position.y += EditorGUIUtility.standardVerticalSpacing;

            // Draw the toggle with bold header style
            toggleProperty.boolValue = EditorGUI.ToggleLeft(position, new GUIContent(toggleHeader.Header),
                toggleProperty.boolValue, EditorStyles.boldLabel);
        }
        else { EditorGUI.LabelField(position, "Toggle field not found."); }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
}