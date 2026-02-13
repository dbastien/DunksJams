using UnityEditor;

//pure insanity there's no right click menu duplicate in unity
public static class DuplicateAsset
{
    [MenuItem("Assets/Duplicate")]
    public static void Duplicate()
    {
        EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));
    }

    [MenuItem("Assets/Duplicate", true)]
    public static bool ValidateDuplicate() => Selection.activeObject != null;
}