using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using System.Reflection;
using CurveType = ComponentMemberReferenceCurve.CurveType;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

[CustomEditor(typeof(ComponentMemberReferenceCurve))]
public class ComponentMemberReferenceCurveEditor : Editor
{
    SerializedProperty _curveProp, _componentProp, _memberProp;
    
    //todo: SerializedProperty[] _startProps, _endProps;
    SerializedProperty _startFloat, _endFloat;
    SerializedProperty _startVector2, _endVector2;
    SerializedProperty _startVector3, _endVector3;
    SerializedProperty _startVector4, _endVector4;
    SerializedProperty _startQuaternion, _endQuaternion;
    SerializedProperty _startColor, _endColor;

    SerializedProperty _selectedGameObjectProp;
    //SerializedProperty _relativeModeProp;
    
    Component[] _availableComponents;
    string[] _validMembers = Array.Empty<string>();

    const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance; 

    void OnEnable()
    {
        _componentProp = serializedObject.FindProperty("memberReference.targetComponent");
        _memberProp = serializedObject.FindProperty("memberReference.targetMemberName");
        _selectedGameObjectProp = serializedObject.FindProperty("selectedGameObject");
        
        _curveProp = serializedObject.FindProperty("curve");

        _startFloat = serializedObject.FindProperty("startFloat");
        _endFloat = serializedObject.FindProperty("endFloat");

        _startVector2 = serializedObject.FindProperty("startVector2");
        _endVector2 = serializedObject.FindProperty("endVector2");

        _startVector3 = serializedObject.FindProperty("startVector3");
        _endVector3 = serializedObject.FindProperty("endVector3");

        _startVector4 = serializedObject.FindProperty("startVector4");
        _endVector4 = serializedObject.FindProperty("endVector4");

        _startQuaternion = serializedObject.FindProperty("startQuaternion");
        _endQuaternion = serializedObject.FindProperty("endQuaternion");

        _startColor = serializedObject.FindProperty("startColor");
        _endColor = serializedObject.FindProperty("endColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_curveProp, new GUIContent("Animation Curve"));

        //relative mode support
        
        EditorGUILayout.PropertyField(_selectedGameObjectProp, new GUIContent("Target GameObject"));
        GameObject selectedGameObject = _selectedGameObjectProp.objectReferenceValue as GameObject;
        
        if (selectedGameObject)
        {
            _availableComponents = selectedGameObject.GetComponents<Component>();
            string[] componentNames = _availableComponents
                .Select(c => c.GetType().Name)
                .ToArray();

            // Display components and select the one referenced in memberReference
            int currentIndex = _availableComponents
                .ToList()
                .IndexOf((Component)_componentProp.objectReferenceValue);
            
            if (currentIndex == -1) currentIndex = 0; // Default to first if not found
            
            int selectedIndex = EditorGUILayout.Popup("Target Component", currentIndex, componentNames);
            if (selectedIndex >= 0 && selectedIndex < _availableComponents.Length)
            {
                _componentProp.objectReferenceValue = _availableComponents[selectedIndex];
                UpdateValidMembers(_availableComponents[selectedIndex]);

                if (_validMembers.Length > 0)
                {
                    int memberIndex = Mathf.Max(Array.IndexOf(_validMembers, _memberProp.stringValue), 0);
                    memberIndex = EditorGUILayout.Popup("Target Member", memberIndex, _validMembers);
                    _memberProp.stringValue = _validMembers[memberIndex];

                    SetCurveTypeBasedOnMember(_availableComponents[selectedIndex], _validMembers[memberIndex]);

                    DisplayStartEndFields();
                }
                else
                {
                    EditorGUILayout.HelpBox("No valid members found in target component.", MessageType.Warning);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DisplayStartEndFields()
    {
        var curveTarget = (ComponentMemberReferenceCurve)target;
        SerializedProperty startProp, endProp;

        switch (curveTarget.curveType)
        {
            case CurveType.Float:
                startProp = _startFloat;
                endProp = _endFloat;
                break;
            case CurveType.Vector2:
                startProp = _startVector2;
                endProp = _endVector2;
                break;
            case CurveType.Vector3:
                startProp = _startVector3;
                endProp = _endVector3;
                break;
            case CurveType.Vector4:
                startProp = _startVector4;
                endProp = _endVector4;
                break;
            case CurveType.Quaternion:
                startProp = _startQuaternion;
                endProp = _endQuaternion;
                break;
            case CurveType.Color:
                startProp = _startColor;
                endProp = _endColor;
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        
        EditorGUILayout.PropertyField(startProp, new GUIContent("Start"));
        EditorGUILayout.PropertyField(endProp, new GUIContent("End"));
    }
    
    void UpdateValidMembers(Component targetComponent)
    {
        _validMembers = targetComponent is Renderer renderer
            ? GetShaderPropertyNames(renderer.sharedMaterial)
            : GetComponentMemberNames(targetComponent);
    }
    
    string[] GetComponentMemberNames(Component component)
    {
        Type componentType = component.GetType();

        string[] fields = componentType.GetFields(bindingFlags)
            .Where(f => IsValidType(f.FieldType))
            .Select(f => f.Name)
            .ToArray();

        string[] properties = componentType.GetProperties(bindingFlags)
            .Where(p => IsValidType(p.PropertyType) && p.GetIndexParameters().Length == 0)
            .Select(p => p.Name)
            .ToArray();

        return fields.Concat(properties).ToArray();
    }
    
    string[] GetShaderPropertyNames(Material material)
    {
        if (!material) return Array.Empty<string>();

        Shader shader = material.shader;
        int propCount = shader.GetPropertyCount();
        System.Collections.Generic.List<string> validProperties = new();

        for (var i = 0; i < propCount; ++i)
        {
            ShaderPropertyType propType = shader.GetPropertyType(i);
            if (IsShaderPropertyValid(propType)) validProperties.Add(shader.GetPropertyName(i));
        }

        return validProperties.ToArray();
    }
    
    bool IsValidType(Type type) =>
        type == typeof(float)   || type == typeof(Vector2)    || type == typeof(Vector3) ||
        type == typeof(Vector4) || type == typeof(Quaternion) || type == typeof(Color);

    bool IsShaderPropertyValid(ShaderPropertyType propType) => 
        propType is
        ShaderPropertyType.Float or ShaderPropertyType.Range or 
        ShaderPropertyType.Color or ShaderPropertyType.Vector;

    void SetCurveTypeBasedOnMember(Component targetComponent, string selectedMember)
    {
        if (targetComponent is Renderer renderer)
        {
            var curveTarget = (ComponentMemberReferenceCurve)target;
            Material mat = curveTarget.memberReference.GetMaterial(renderer);
            if (mat && mat.HasProperty(selectedMember))
            {
                // Map shader property type to curve type
                Shader shader = mat.shader;
                int propertyIndex = shader.FindPropertyIndex(selectedMember);
                if (propertyIndex < 0)
                {
                    DLog.LogE($"Shader property '{selectedMember}' not found.");
                    return;
                }
                
                ShaderPropertyType propertyType = shader.GetPropertyType(propertyIndex);
                switch (propertyType)
                {
                    case ShaderPropertyType.Color:
                        curveTarget.curveType = CurveType.Color;
                        break;
                    case ShaderPropertyType.Float: 
                    case ShaderPropertyType.Range:
                        curveTarget.curveType = CurveType.Float;
                        break;
                    case ShaderPropertyType.Vector:
                        curveTarget.curveType = CurveType.Vector4;  //shader vecs are all V4s in Unity
                        break;
                    default: DLog.LogE($"Shader property '{selectedMember}' is unsupported type '{propertyType}'.");
                        break;
                }
            }
            else
            {
                DLog.LogE($"Material is null or does not have property '{selectedMember}'.");
            }
        }
        else
        {
            // Handle non-renderer components by inspecting the type of the selected field/property
            Type componentType = targetComponent.GetType();
            FieldInfo fieldInfo = componentType.GetField(selectedMember, bindingFlags);
            PropertyInfo propertyInfo = componentType.GetProperty(selectedMember, bindingFlags);

            Type memberType = fieldInfo?.FieldType ?? propertyInfo?.PropertyType;

            if (memberType != null)
            {
                var curveTarget = (ComponentMemberReferenceCurve)target;

                if      (memberType == typeof(float))      curveTarget.curveType = CurveType.Float;
                else if (memberType == typeof(Vector2))    curveTarget.curveType = CurveType.Vector2;
                else if (memberType == typeof(Vector3))    curveTarget.curveType = CurveType.Vector3;
                else if (memberType == typeof(Vector4))    curveTarget.curveType = CurveType.Vector4;
                else if (memberType == typeof(Quaternion)) curveTarget.curveType = CurveType.Quaternion;
                else if (memberType == typeof(Color))      curveTarget.curveType = CurveType.Color;
            }
            else
            {
                DLog.LogE($"Member '{selectedMember}' not found on component '{targetComponent.GetType().Name}'.");
            }
        }
    }
}
