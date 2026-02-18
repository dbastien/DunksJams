using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using ShaderPropertyType = UnityEngine.Rendering.ShaderPropertyType;

[Serializable]
public class ComponentMemberReference
{
    [SerializeField] public Component targetComponent;
    [SerializeField] private string targetMemberName;
    [SerializeField] private int materialIndex;

    private FieldInfo _fieldInfo;
    private PropertyInfo _propInfo;
    private bool _infoCached;
    private MaterialPropertyBlock _materialPropBlock;
    private ShaderPropertyType? _shaderPropType;

    // Cached delegates for performance (avoids reflection every frame)
    private Action<object> _cachedSetter;
    private Func<object> _cachedGetter;

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
                ShaderPropertyType propertyType =
                    material.shader.GetPropertyType(material.shader.FindPropertyIndex(targetMemberName));

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

    private bool CacheInfo()
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

    private void CreateCachedDelegates(Type componentType)
    {
        // Create compiled getter/setter using expression trees for ~50x faster access than reflection
        ConstantExpression targetParam = Expression.Constant(targetComponent);

        if (_fieldInfo != null)
        {
            // Getter: () => ((ComponentType)target).fieldName
            MemberExpression fieldAccess = Expression.Field(Expression.Convert(targetParam, componentType), _fieldInfo);
            Expression<Func<object>> getterLambda =
                Expression.Lambda<Func<object>>(Expression.Convert(fieldAccess, typeof(object)));
            _cachedGetter = getterLambda.Compile();

            // Setter: (val) => ((ComponentType)target).fieldName = (FieldType)val
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            BinaryExpression assignExpr =
                Expression.Assign(fieldAccess, Expression.Convert(valueParam, _fieldInfo.FieldType));
            Expression<Action<object>> setterLambda = Expression.Lambda<Action<object>>(assignExpr, valueParam);
            _cachedSetter = setterLambda.Compile();
        }
        else if (_propInfo != null)
        {
            // Getter: () => ((ComponentType)target).propertyName
            MemberExpression propAccess =
                Expression.Property(Expression.Convert(targetParam, componentType), _propInfo);
            Expression<Func<object>> getterLambda =
                Expression.Lambda<Func<object>>(Expression.Convert(propAccess, typeof(object)));
            _cachedGetter = getterLambda.Compile();

            // Setter: (val) => ((ComponentType)target).propertyName = (PropertyType)val
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            BinaryExpression assignExpr =
                Expression.Assign(propAccess, Expression.Convert(valueParam, _propInfo.PropertyType));
            Expression<Action<object>> setterLambda = Expression.Lambda<Action<object>>(assignExpr, valueParam);
            _cachedSetter = setterLambda.Compile();
        }
    }
}