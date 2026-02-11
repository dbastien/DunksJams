using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using Utilities;

public class CopyCatWindow : EditorWindow
{
    static Object _src;
    static List<FieldInfo> _fields;
    static List<PropertyInfo> _props;
    static object[] _fieldVals, _propVals;
    static bool[] _fieldSelected, _propSelected;

    static bool _nonHomogeneous;
    Vector2 _scrollPos;

    [MenuItem("CONTEXT/Component/CopyCat")]
    static void CopyMenuSelected(MenuCommand cmd)
    {
        var win = GetWindow<CopyCatWindow>(true, "CopyCat", true);
        win.Init(cmd.context);  // Init w/ source object
    }

    void Init(Object src)
    {
        _src = src;
        var type = _src.GetType();

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
    static void PasteMenuSelected(MenuCommand command)
    {
        var win = GetWindow<CopyCatWindow>();
        
        if (_fields == null || _props == null || _fieldVals == null || _propVals == null)
        {
            DLog.LogW("No source data, copy before pasting.");
            return;
        }
        
        win.Paste(command.context, _nonHomogeneous);
    }

    void Paste(Object target, bool nonHomogeneous)
    {
        if (_fields == null || _props == null || _fieldVals == null || _propVals == null)
        {
            DLog.LogW("No source data, copy before pasting.");
            return;
        }
        
        Undo.RecordObject(target, "PasteCat Data");

        var tgtFields = target.GetType().GetFields();
        var tgtProps = target.GetType().GetAllProperties().ToArray();
        
        for (int i = 0; i < _fieldSelected.Length; ++i)
        {
            if (!_fieldSelected[i] || _fieldVals[i] == null) continue;
            var srcField = _fields[i];
            var tgtField = System.Array.Find(tgtFields, f => f.Name == srcField.Name)
                              ?? (nonHomogeneous ? System.Array.Find(tgtFields, f => f.FieldType.IsAssignableTo(srcField.FieldType)) : null);

            if (tgtField == null) continue;
            tgtField.SetValue(target, _fieldVals[i].DeepClone());
        }

        for (int i = 0; i < _propSelected.Length; ++i)
        {
            if (!_propSelected[i] || _propVals[i] == null) continue;
            var srcProp = _props[i];
            var tgtProp = System.Array.Find(tgtProps, p => p.Name == srcProp.Name)
                             ?? (nonHomogeneous ? System.Array.Find(tgtProps, p => p.PropertyType.IsAssignableTo(srcProp.PropertyType)) : null);

            if (tgtProp == null) continue;
            tgtProp.SetValue(target, _propVals[i].DeepClone());
        }

        EditorUtility.SetDirty(target);
        SceneView.RepaintAll();
    }

    void OnGUICopy()
    {
        _nonHomogeneous = EditorGUILayout.Toggle("Non-Homogeneous", _nonHomogeneous);

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));

        EditorGUILayout.LabelField("Fields");
        for (int i = 0; i < _fields.Count; ++i)
            _fieldSelected[i] = EditorGUILayout.Toggle(_fields[i].Name, _fieldSelected[i]);

        EditorGUILayout.LabelField("Properties");
        for (int i = 0; i < _props.Count; ++i)
            _propSelected[i] = EditorGUILayout.Toggle(_props[i].Name, _propSelected[i]);

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Copy"))
        {
            for (int i = 0; i < _fields.Count; ++i)
                if (_fieldSelected[i]) _fieldVals[i] = _fields[i].GetValue(_src);

            for (int i = 0; i < _props.Count; ++i)
                if (_propSelected[i]) _propVals[i] = _props[i].GetValue(_src);

            Close();
        }
    }

    void OnGUI() => OnGUICopy();
}