using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformCurve))]
public class TransformCurveEditor : Editor
{
    private static List<string> _propNames = new();
    private static string[] _propNamesArray;

    static TransformCurveEditor()
    {
        MemberInfo[] members = typeof(Transform).GetMembers(BindingFlags.SetProperty |
                                                            BindingFlags.GetProperty |
                                                            BindingFlags.Instance |
                                                            BindingFlags.Public);

        foreach (MemberInfo member in members)
        {
            if (member.MemberType != MemberTypes.Property) continue;
            var info = member as PropertyInfo;
            if (info?.PropertyType == typeof(Vector3)) _propNames.Add(info.Name);
        }

        _propNamesArray = _propNames.ToArray();
    }

    public override void OnInspectorGUI()
    {
        var targetCurve = target as TransformCurve;

        int index = _propNames.IndexOf(targetCurve.curveTargetName);
        index = Mathf.Max(0, index);

        serializedObject.Update();
        SerializedProperty curveProperty = serializedObject.FindProperty("Curve");

        EditorGUILayout.PropertyField(curveProperty);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUI.BeginChangeCheck();
                {
                    index = EditorGUILayout.Popup(index, _propNamesArray);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    targetCurve.curveTargetName = _propNamesArray[index];
                    targetCurve.ResetStartEnd();
                }

                EditorGUIUtility.labelWidth = 90f;

                targetCurve.relativeMode = EditorGUILayout.Toggle("Relative Mode", targetCurve.relativeMode);

                if (targetCurve.relativeMode)
                {
                    targetCurve.curveOffset = EditorGUILayout.Vector3Field("offset", targetCurve.curveOffset);
                }
                else
                {
                    targetCurve.curveStart = EditorGUILayout.Vector3Field("start", targetCurve.curveStart);
                    targetCurve.curveEnd = EditorGUILayout.Vector3Field("end", targetCurve.curveEnd);
                }

                targetCurve.lengthScale = EditorGUILayout.FloatField("length scale", targetCurve.lengthScale);
                if (targetCurve.lengthScale == 0)
                    EditorGUILayout.HelpBox("Length scale cannot be zero!", MessageType.Error);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        //reset to default
        EditorGUIUtility.labelWidth = 0;
    }
}