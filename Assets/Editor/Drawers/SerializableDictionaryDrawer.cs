using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    bool _foldout;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        _foldout = EditorGUILayout.Foldout(_foldout, label, true);
        if (!_foldout) return;

        EditorGUI.indentLevel++;

        SerializedProperty keys = property.FindPropertyRelative("_keys");
        SerializedProperty values = property.FindPropertyRelative("_values");

        if (GUILayout.Button("Add Entry"))
        {
            int newIndex = keys.arraySize;
            keys.InsertArrayElementAtIndex(newIndex);
            values.InsertArrayElementAtIndex(newIndex);

            keys.GetArrayElementAtIndex(newIndex).SetDefaultValue();
            values.GetArrayElementAtIndex(newIndex).SetDefaultValue();

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            GUI.FocusControl(null);
        }

        for (int i = 0; i < keys.arraySize; ++i)
        {
            EditorGUILayout.BeginHorizontal();

            if (i < keys.arraySize && i < values.arraySize)
            {
                EditorGUILayout.PropertyField(keys.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUILayout.PropertyField(values.GetArrayElementAtIndex(i), GUIContent.none);

                if (GUILayout.Button("Remove"))
                {
                    RemoveElementAtIndex(keys, i);
                    RemoveElementAtIndex(values, i);

                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    GUI.FocusControl(null);
                    break;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    private void RemoveElementAtIndex(SerializedProperty array, int index)
    {
        array.DeleteArrayElementAtIndex(index);
        if (array.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (index < array.arraySize && array.GetArrayElementAtIndex(index).objectReferenceValue == null)
                array.DeleteArrayElementAtIndex(index);
        }
    }
}