using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformCurve))]
public class TransformCurveEditor : Editor
{
    static List<string> _propNames = new();
    static string[] _propNamesArray;

    static TransformCurveEditor()
    {
        var members = typeof(Transform).GetMembers(BindingFlags.SetProperty | BindingFlags.GetProperty |
                                                   BindingFlags.Instance | BindingFlags.Public);

        foreach (var member in members)
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

        var index = _propNames.IndexOf(targetCurve.curveTargetName);
        index = Mathf.Max(0, index);

        serializedObject.Update();
        var curveProperty = serializedObject.FindProperty("Curve");

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