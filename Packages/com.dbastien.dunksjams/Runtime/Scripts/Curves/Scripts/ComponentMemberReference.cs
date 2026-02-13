using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

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

    // Cached delegates for performance (avoids reflection every frame)
    Action<object> _cachedSetter;
    Func<object> _cachedGetter;

    public string GetTargetMemberName() => targetMemberName;

    public bool IsValid() => targetComponent != null && !string.IsNullOrEmpty(targetMemberName) && CacheInfo();

    public object GetValue()
    {
        if (!CacheInfo()) return null;

        if (targetComponent is Renderer renderer)
        {
            var material = GetMaterial(renderer);
            if (material && _shaderPropType.HasValue)
            {
                var propertyType = material.shader.GetPropertyType(material.shader.FindPropertyIndex(targetMemberName));

                switch (propertyType)
                {
                    case ShaderPropertyType.Color: return material.GetColor(targetMemberName);
                    case ShaderPropertyType.Float or ShaderPropertyType.Range:
                        return material.GetFloat(targetMemberName);
                    case ShaderPropertyType.Vector: return material.GetVector(targetMemberName);
                }
            }
            else
            {
                DLog.LogE($"Material does not have the property '{targetMemberName}'.");
                return null;
            }
        }

        return _cachedGetter?.Invoke();
    }

    public void SetValue(object value)
    {
        if (!CacheInfo()) return;

        if (targetComponent is Renderer renderer)
        {
            _materialPropBlock ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_materialPropBlock);

            switch (value)
            {
                case Color val:
                    _materialPropBlock.SetColor(targetMemberName, val);
                    break;
                case float val:
                    _materialPropBlock.SetFloat(targetMemberName, val);
                    break;
                case Vector4 val:
                    _materialPropBlock.SetVector(targetMemberName, val);
                    break;
                default:
                    DLog.Log($"[SetValue] Unsupported type for {targetMemberName}");
                    return;
            }

            renderer.SetPropertyBlock(_materialPropBlock);
        }
        else
        {
            if (_cachedSetter != null)
                _cachedSetter(value);
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

        var type = targetComponent.GetType();
        var cleanMemberName = targetMemberName.Split(' ')[0];

        if (targetComponent is Renderer renderer)
        {
            var mat = GetMaterial(renderer);
            if (mat && mat.HasProperty(cleanMemberName))
            {
                _shaderPropType = mat.shader.GetPropertyType(mat.shader.FindPropertyIndex(cleanMemberName));
                _infoCached = true;
                return true;
            }
        }

        _fieldInfo = type.GetField(cleanMemberName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _propInfo = type.GetProperty(cleanMemberName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (_fieldInfo == null && _propInfo == null)
        {
            DLog.LogE($"Member '{cleanMemberName}' not found on {type.Name}.");
            _infoCached = true;
            return false;
        }

        // Create compiled delegates using expression trees (avoids reflection at runtime)
        CreateCachedDelegates(type);

        _infoCached = true;
        return _fieldInfo != null || _propInfo != null;
    }

    public Material GetMaterial(Renderer renderer) => renderer.sharedMaterials[materialIndex];

    void CreateCachedDelegates(Type componentType)
    {
        // Create compiled getter/setter using expression trees for ~50x faster access than reflection
        var targetParam = Expression.Constant(targetComponent);

        if (_fieldInfo != null)
        {
            // Getter: () => ((ComponentType)target).fieldName
            var fieldAccess = Expression.Field(Expression.Convert(targetParam, componentType), _fieldInfo);
            var getterLambda = Expression.Lambda<Func<object>>(Expression.Convert(fieldAccess, typeof(object)));
            _cachedGetter = getterLambda.Compile();

            // Setter: (val) => ((ComponentType)target).fieldName = (FieldType)val
            var valueParam = Expression.Parameter(typeof(object), "value");
            var assignExpr = Expression.Assign(fieldAccess, Expression.Convert(valueParam, _fieldInfo.FieldType));
            var setterLambda = Expression.Lambda<Action<object>>(assignExpr, valueParam);
            _cachedSetter = setterLambda.Compile();
        }
        else if (_propInfo != null)
        {
            // Getter: () => ((ComponentType)target).propertyName
            var propAccess = Expression.Property(Expression.Convert(targetParam, componentType), _propInfo);
            var getterLambda = Expression.Lambda<Func<object>>(Expression.Convert(propAccess, typeof(object)));
            _cachedGetter = getterLambda.Compile();

            // Setter: (val) => ((ComponentType)target).propertyName = (PropertyType)val
            var valueParam = Expression.Parameter(typeof(object), "value");
            var assignExpr = Expression.Assign(propAccess, Expression.Convert(valueParam, _propInfo.PropertyType));
            var setterLambda = Expression.Lambda<Action<object>>(assignExpr, valueParam);
            _cachedSetter = setterLambda.Compile();
        }
    }
}