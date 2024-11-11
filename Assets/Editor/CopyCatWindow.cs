using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Rendering;

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
    static void CopyMenuSelected(MenuCommand command)
    {
        var win = GetWindow<CopyCatWindow>(true, "CopyCat", true);
        win.Init(command.context);  // Init with source object
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

        var targetFields = target.GetType().GetFields();
        var targetProps = target.GetType().GetAllProperties().ToArray();
        
        for (int i = 0; i < _fieldSelected.Length; ++i)
        {
            if (!_fieldSelected[i] || _fieldVals[i] == null) continue;
            var sourceField = _fields[i];
            var targetField = System.Array.Find(targetFields, f => f.Name == sourceField.Name)
                              ?? (nonHomogeneous ? System.Array.Find(targetFields, f => f.FieldType.IsAssignableTo(sourceField.FieldType)) : null);

            if (targetField == null) continue;
            targetField.SetValue(target, _fieldVals[i].DeepClone());
        }

        for (int i = 0; i < _propSelected.Length; ++i)
        {
            if (!_propSelected[i] || _propVals[i] == null) continue;
            var sourceProp = _props[i];
            var targetProp = System.Array.Find(targetProps, p => p.Name == sourceProp.Name)
                             ?? (nonHomogeneous ? System.Array.Find(targetProps, p => p.PropertyType.IsAssignableTo(sourceProp.PropertyType)) : null);

            if (targetProp == null) continue;
            targetProp.SetValue(target, _propVals[i].DeepClone());
        }

        EditorUtility.SetDirty(target);
        SceneView.RepaintAll();
    }

    void OnGUICopy()
    {
        _nonHomogeneous = EditorGUILayout.Toggle("Non-Homogeneous", _nonHomogeneous);

        // Start scroll view
        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));

        EditorGUILayout.LabelField("Fields");
        for (int i = 0; i < _fields.Count; ++i)
            _fieldSelected[i] = EditorGUILayout.Toggle(_fields[i].Name, _fieldSelected[i]);

        EditorGUILayout.LabelField("Properties");
        for (int i = 0; i < _props.Count; ++i)
            _propSelected[i] = EditorGUILayout.Toggle(_props[i].Name, _propSelected[i]);

        EditorGUILayout.EndScrollView();  // End scroll view

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
