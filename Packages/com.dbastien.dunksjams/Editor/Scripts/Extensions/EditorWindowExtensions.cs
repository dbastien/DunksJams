#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EditorWindowExtensions
{
    public static void MoveTo(this EditorWindow window, Vector2 position, bool ensureFitsOnScreen = true)
    {
        if (!ensureFitsOnScreen)
        {
            window.position = window.position.SetPos(position);
            return;
        }

        Rect windowRect = window.position;
        Rect unityWindowRect = EditorGUIUtility.GetMainWindowPosition();

        position.x = position.x.Max(unityWindowRect.position.x);
        position.y = position.y.Max(unityWindowRect.position.y);

        position.x = position.x.Min(unityWindowRect.xMax - windowRect.width);
        position.y = position.y.Min(unityWindowRect.yMax - windowRect.height);

        window.position = windowRect.SetPos(position);
    }
}
#endif