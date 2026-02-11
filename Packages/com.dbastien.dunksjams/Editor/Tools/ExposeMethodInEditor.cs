using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Utilities;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ExposeMethodInEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var methods = target.GetType().GetMethodsWithAttribute<ExposeMethodInEditorAttribute>().ToList();

        if (methods.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Exposed Methods", EditorStyles.boldLabel);

        foreach (MethodInfo method in methods)
            if (GUILayout.Button(method.Name)) method.Invoke(target, null);
    }
}