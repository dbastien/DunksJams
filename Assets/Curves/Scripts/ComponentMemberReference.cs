using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ShaderPropertyType = UnityEditor.ShaderUtil.ShaderPropertyType;

[Serializable]
public class ComponentMemberReference
{
    [SerializeField] public Component targetComponent;
    [SerializeField] string targetMemberName;
    [SerializeField] int materialIndex;

    FieldInfo _fieldInfo;
    PropertyInfo _propInfo;
    bool _infoCached;
    MaterialPropertyBlock _materialPropBlock;
    ShaderPropertyType? _shaderPropType;
    
    public string GetTargetMemberName() => targetMemberName;

    public bool IsValid() => targetComponent != null && !string.IsNullOrEmpty(targetMemberName) && CacheInfo();

    public object GetValue()
    {
        if (!CacheInfo()) return null;

        if (targetComponent is Renderer renderer)
        {
            Material material = GetMaterial(renderer);
            if (material && _shaderPropType.HasValue)
            {
                ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(material.shader, material.shader.FindPropertyIndex(targetMemberName));
        
                switch (propertyType)
                {
                    case ShaderPropertyType.Color: return material.GetColor(targetMemberName);
                    case ShaderPropertyType.Float or ShaderPropertyType.Range: return material.GetFloat(targetMemberName);
                    case ShaderPropertyType.Vector: return material.GetVector(targetMemberName);
                }
            }
            else
            {
                DLog.LogE($"Material does not have the property '{targetMemberName}'.");
                return null;
            }
        }

        return _fieldInfo != null ? _fieldInfo.GetValue(targetComponent) : _propInfo.GetValue(targetComponent, null);
    }

    public void SetValue(object value)
    {
        if (!CacheInfo()) return;

        if (targetComponent is Renderer renderer)
        {
            _materialPropBlock ??= new();
            renderer.GetPropertyBlock(_materialPropBlock);

            switch (value)
            {
                case Color val: _materialPropBlock.SetColor(targetMemberName, val);
                    break;
                case float val: _materialPropBlock.SetFloat(targetMemberName, val);
                    break;
                case Vector4 val: _materialPropBlock.SetVector(targetMemberName, val);
                    break;
                default:
                    DLog.Log($"[SetValue] Unsupported type for {targetMemberName}");
                    return;
            }

            renderer.SetPropertyBlock(_materialPropBlock);
        }
        else
        {
            if (_fieldInfo != null)
                _fieldInfo.SetValue(targetComponent, value);
            else if (_propInfo != null)
                _propInfo.SetValue(targetComponent, value, null);
            else
                DLog.LogE($"[SetValue] Target member '{targetMemberName}' not found on target component.");
        }
    }

    bool CacheInfo()
    {
        if (_infoCached) return true;
        //if (_infoCached) return _fieldInfo != null || _propertyInfo != null;
        if (!targetComponent || string.IsNullOrEmpty(targetMemberName))
        {
            DLog.LogE("Target component or member name is not set.");
            return false;
        }
        
        Type type = targetComponent.GetType();
        string cleanMemberName = targetMemberName.Split(' ')[0];

        if (targetComponent is Renderer renderer)
        {
            Material mat = GetMaterial(renderer);
            if (mat && mat.HasProperty(cleanMemberName))
            {
                _shaderPropType = ShaderUtil.GetPropertyType(mat.shader, mat.shader.FindPropertyIndex(cleanMemberName));
                _infoCached = true;
                return true;
            }
        }

        _fieldInfo = type.GetField(cleanMemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _propInfo = type.GetProperty(cleanMemberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (_fieldInfo == null && _propInfo == null) DLog.LogE($"Member '{cleanMemberName}' not found on {type.Name}.");

        _infoCached = true;
        return _fieldInfo != null || _propInfo != null;
    }

    public Material GetMaterial(Renderer renderer) => renderer.sharedMaterials[materialIndex];
}