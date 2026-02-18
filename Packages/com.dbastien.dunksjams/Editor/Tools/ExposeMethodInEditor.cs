using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ExposeMethodInEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        List<MethodInfo> methods = target.GetType().GetMethodsWithAttribute<ExposeMethodInEditorAttribute>().ToList();

        if (methods.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Exposed Methods", EditorStyles.boldLabel);

        foreach (MethodInfo method in methods)
            if (GUILayout.Button(method.Name))
                method.Invoke(target, null);
    }
}