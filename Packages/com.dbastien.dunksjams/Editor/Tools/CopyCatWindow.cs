using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

public class CopyCatWindow : EditorWindow
{
    private static Object _src;
    private static List<FieldInfo> _fields;
    private static List<PropertyInfo> _props;
    private static object[] _fieldVals, _propVals;
    private static bool[] _fieldSelected, _propSelected;

    private static bool _nonHomogeneous;
    private Vector2 _scrollPos;

    [MenuItem("CONTEXT/Component/CopyCat")]
    private static void CopyMenuSelected(MenuCommand cmd)
    {
        var win = GetWindow<CopyCatWindow>(true, "CopyCat", true);
        win.Init(cmd.context); // Init w/ source object
    }

    private void Init(Object src)
    {
        _src = src;
        Type type = _src.GetType();

        _fields = type.GetSerializableFields();
        _props = type.GetAllProperties();

        _fieldVals = new object[_fields.Count];
        _fieldSelected = new bool[_fields.Count];
        _fieldSelected.Fill(true);
        _propVals = new object[_props.Count];
        _propSelected = new bool[_props.Count];
        _propSelected.Fill(true);
    }

    [MenuItem("CONTEXT/Component/PasteCat")]
    private static void PasteMenuSelected(MenuCommand command)
    {
        var win = GetWindow<CopyCatWindow>();

        if (_fields == null || _props == null || _fieldVals == null || _propVals == null)
        {
            DLog.LogW("No source data, copy before pasting.");
            return;
        }

        win.Paste(command.context, _nonHomogeneous);
    }

    private void Paste(Object target, bool nonHomogeneous)
    {
        if (_fields == null || _props == null || _fieldVals == null || _propVals == null)
        {
            DLog.LogW("No source data, copy before pasting.");
            return;
        }

        Undo.RecordObject(target, "PasteCat Data");

        FieldInfo[] tgtFields = target.GetType().GetFields();
        PropertyInfo[] tgtProps = target.GetType().GetAllProperties().ToArray();

        for (var i = 0; i < _fieldSelected.Length; ++i)
        {
            if (!_fieldSelected[i] || _fieldVals[i] == null) continue;
            FieldInfo srcField = _fields[i];
            FieldInfo tgtField = Array.Find(tgtFields, f => f.Name == srcField.Name) ??
                                 (nonHomogeneous
                                     ? Array.Find(tgtFields, f => f.FieldType.IsAssignableTo(srcField.FieldType))
                                     : null);

            if (tgtField == null) continue;
            tgtField.SetValue(target, _fieldVals[i].DeepClone());
        }

        for (var i = 0; i < _propSelected.Length; ++i)
        {
            if (!_propSelected[i] || _propVals[i] == null) continue;
            PropertyInfo srcProp = _props[i];
            PropertyInfo tgtProp = Array.Find(tgtProps, p => p.Name == srcProp.Name) ??
                                   (nonHomogeneous
                                       ? Array.Find(tgtProps,
                                           p => p.PropertyType.IsAssignableTo(srcProp.PropertyType))
                                       : null);

            if (tgtProp == null) continue;
            tgtProp.SetValue(target, _propVals[i].DeepClone());
        }

        EditorUtility.SetDirty(target);
        SceneView.RepaintAll();
    }

    private void OnGUICopy()
    {
        _nonHomogeneous = EditorGUILayout.Toggle("Non-Homogeneous", _nonHomogeneous);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width),
            GUILayout.Height(position.height - 50));

        EditorGUILayout.LabelField("Fields");
        for (var i = 0; i < _fields.Count; ++i)
            _fieldSelected[i] = EditorGUILayout.Toggle(_fields[i].Name, _fieldSelected[i]);

        EditorGUILayout.LabelField("Properties");
        for (var i = 0; i < _props.Count; ++i)
            _propSelected[i] = EditorGUILayout.Toggle(_props[i].Name, _propSelected[i]);

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Copy"))
        {
            for (var i = 0; i < _fields.Count; ++i)
                if (_fieldSelected[i])
                    _fieldVals[i] = _fields[i].GetValue(_src);

            for (var i = 0; i < _props.Count; ++i)
                if (_propSelected[i])
                    _propVals[i] = _props[i].GetValue(_src);

            Close();
        }
    }

    private void OnGUI() => OnGUICopy();
}