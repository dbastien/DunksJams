using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ReflectionUtils
{
    public const BindingFlags All = BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.Static;

    public const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    // Tabify compatibility: maxBindingFlags = 62 = (Public|NonPublic|Instance|Static|FlattenHierarchy)
    public const BindingFlags maxBindingFlags = (BindingFlags)62;

    private static readonly HashSet<string> ExcludedPropertyNames =
        new() { "runInEditMode", "useGUILayout", "hideFlags" };

    private static readonly Dictionary<(Type, string), FieldInfo> _fieldCache = new();
    private static readonly Dictionary<(Type, string, Type[]), MethodInfo> _methodCache = new();
    private static readonly Dictionary<(Type, string), PropertyInfo> _propCache = new();
    private static readonly Dictionary<Type, List<Type>> _derivedCache = new();

    //todo maybe switch to eager init?
    private static readonly Lazy<IEnumerable<Type>> _allTypes =
        new(() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()));

    private static readonly char[] _delims = { ',', '[' };

    public static List<Type> GetTypesWithPrefix(string prefix) =>
        _allTypes.Value.Where(t => t.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

    public static Type GetTypeByName(string name) =>
        _allTypes.Value.FirstOrDefault(t => t.Name.Equals(name, StringComparison.Ordinal));

    public static IEnumerable<Type> GetTypesWithFlags(BindingFlags flags) =>
        _allTypes.Value.Where(t => t.IsClass && t.GetFields(flags).Any());

    public static List<Type> GetNonGenericDerivedTypes<T>() =>
        _derivedCache.GetOrAdd(typeof(T), _ =>
            _allTypes.Value.Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract).ToList());

    public static List<Type> GetDerived(this Type baseType) =>
        _derivedCache.GetOrAdd(baseType, _ =>
            _allTypes.Value.Where(t => IsGenericBase(baseType, t) && !t.IsAbstract && !t.ContainsGenericParameters).
                ToList());

    public static bool IsGenericBase(Type generic, Type sub)
    {
        while (sub != null && sub != typeof(object))
        {
            if (sub.IsGenericType && sub.GetGenericTypeDefinition() == generic) return true;
            sub = sub.BaseType;
        }

        return false;
    }

    public static IEnumerable<Type> GetHierarchy(this Type t, bool includeInterfaces = true) =>
        t.GetInterfaces().Concat(Enumerable.Repeat(t.BaseType, 1).Where(b => b != null));

    public static IEnumerable<T> GetVariableValues<T>(this object obj) =>
        obj.GetType().
            GetFields(All).
            Where(f => typeof(T).IsAssignableFrom(f.FieldType)).
            Select(f => f.GetValue(obj)).
            OfType<T>();

    public static void CopyFields(object src, object dst, bool deep = false)
    {
        if (src == null || dst == null) throw new ArgumentNullException();
        foreach (FieldInfo f in dst.GetType().GetFields(All))
        {
            if (f.IsLiteral || f.IsInitOnly) continue;
            FieldInfo srcField = _fieldCache.GetOrAdd((src.GetType(), f.Name), t => t.Item1.GetField(t.Item2, All));
            object val = srcField?.GetValue(src);
            f.SetValue(dst, deep && val != null && val.GetType().IsClass ? val.DeepClone() : val);
        }
    }

    public static object DeepClone(this object obj)
    {
        if (obj == null) return null;
        object clone = Activator.CreateInstance(obj.GetType());
        CopyFields(obj, clone, true);
        return clone;
    }

    public static string TypeName(string name) =>
        name.IndexOfAny(_delims) is var i && i == -1 ? name : name[..(FindBracket(name.AsSpan(), i) + 1)];

    public static IEnumerable<string> GetGenericParams(string name)
    {
        for (int openPos = name.IndexOf('['); openPos >= 0; openPos = name.IndexOf('[', openPos + 1))
        {
            int closePos = FindBracket(name.AsSpan(), openPos);
            yield return TypeName(name[(openPos + 1)..closePos]);
        }
    }

    private static int FindBracket(ReadOnlySpan<char> s, int start)
    {
        int depth = 0, i = start;
        do { depth += s[i++] switch { '[' => 1, ']' => -1, _ => 0 }; }
        while (depth > 0);

        return i - 1;
    }

    // Extension methods for Tabify compatibility (non-generic)
    public static object GetFieldValue(this object obj, string name)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        return GetField(type, name)?.GetValue(target);
    }

    public static void SetFieldValue(this object obj, string name, object val)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        GetField(type, name)?.SetValue(target, val);
    }

    public static object GetPropertyValue(this object obj, string name)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        return GetProp(type, name)?.GetValue(target);
    }

    public static void SetPropertyValue(this object obj, string name, object val)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        GetProp(type, name)?.SetValue(target, val);
    }

    public static object GetMemberValue(this object obj, string name)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        return GetField(type, name)?.GetValue(target) ?? GetProp(type, name)?.GetValue(target);
    }

    public static void SetMemberValue(this object obj, string name, object val)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        FieldInfo field = GetField(type, name);
        if (field != null)
        {
            field.SetValue(target, val);
            return;
        }

        GetProp(type, name)?.SetValue(target, val);
    }

    public static object InvokeMethod(this object obj, string name, params object[] args)
    {
        Type type = obj as Type ?? obj.GetType();
        object target = obj is Type ? null : obj;
        return GetMethod(type, name, args.Select(a => a?.GetType()).ToArray())?.Invoke(target, args);
    }

    // Generic overloads
    public static T GetFieldValue<T>(this object obj, string name) => (T)obj.GetFieldValue(name);
    public static T GetPropertyValue<T>(this object obj, string name) => (T)obj.GetPropertyValue(name);
    public static T GetMemberValue<T>(this object obj, string name) => (T)obj.GetMemberValue(name);

    public static T InvokeMethod<T>
        (this object obj, string name, params object[] args) => (T)obj.InvokeMethod(name, args);

    public static FieldInfo GetField(Type type, string name, BindingFlags flags = All) =>
        _fieldCache.GetOrAdd((type, name), t => t.Item1.GetField(t.Item2, flags));

    public static PropertyInfo GetProp(Type type, string name, BindingFlags flags = All) =>
        _propCache.GetOrAdd((type, name), t => t.Item1.GetProperty(t.Item2, flags));

    public static PropertyInfo GetPropertyInfo
        (this Type type, string name, BindingFlags flags = All) => GetProp(type, name, flags);

    public static MethodInfo GetMethod(Type type, string name, Type[] argTypes) =>
        _methodCache.GetOrAdd((type, name, argTypes),
            t => t.Item1.GetMethods(All).
                FirstOrDefault(m => m.Name == t.Item2 && MatchParams(m.GetParameters(), t.Item3)));

    public static string GetMethodSignature(this MethodInfo mi) =>
        $"{mi.Name}({string.Join(", ", mi.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"))})";

    public static object InvokeMethod(object obj, MethodInfo mi, params object[] args) => mi?.Invoke(obj, args);

    public static FieldInfo[] GetFields(this Type t, BindingFlags flags = All) => t.GetFields(flags);

    public static List<PropertyInfo> GetAllProperties(this Type t, BindingFlags flags = AllInstance)
    {
        List<PropertyInfo> props = new();
        foreach (PropertyInfo prop in t.GetProperties(flags))
            if (prop.CanRead && prop.CanWrite)
                props.Add(prop);

        return props;
    }

    public static object CreateInstance(this Type t, params object[] args) =>
        typeof(ScriptableObject).IsAssignableFrom(t)
            ? ScriptableObject.CreateInstance(t)
            : Activator.CreateInstance(t, args);

    private static bool MatchParams(ParameterInfo[] pi, Type[] types)
    {
        if (pi.Length != types.Length) return false;
        for (var i = 0; i < types.Length; ++i)
            if (!pi[i].ParameterType.IsAssignableFrom(types[i]))
                return false;

        return true;
    }

    public static ConstructorInfo GetConstructor(Type t, params Type[] types) =>
        t.GetConstructor(All, null, types, null);

    public static bool IsCompatible(this FieldInfo fi1, FieldInfo fi2) => fi1.FieldType == fi2.FieldType;

    public static bool IsCompatible(this PropertyInfo pi1, PropertyInfo pi2) =>
        pi1.PropertyType == pi2.PropertyType;

    public static bool IsAssignableTo(this Type t, Type target) => target.IsAssignableFrom(t);
    public static bool IsAssignableTo<T>(this Type t) => typeof(T).IsAssignableFrom(t);

    public static IEnumerable<PropertyInfo> GetStaticProperties(this Type t, BindingFlags flags = All) =>
        t.GetProperties(flags).Where(p => p.GetGetMethod(true)?.IsStatic ?? false);

    public static IEnumerable<FieldInfo> GetStaticFields(this Type t, BindingFlags flags = All) =>
        t.GetFields(flags).Where(f => f.IsStatic);

    public static void SetValue(this MemberInfo mi, object inst, object val)
    {
        if (mi is PropertyInfo prop) prop.SetValue(inst, val);
        else if (mi is FieldInfo field) field.SetValue(inst, val);
        else throw new InvalidOperationException("Member is not a property or field.");
    }

    public static object GetValue(this MemberInfo mi, object inst) =>
        mi is PropertyInfo pi ? pi.GetValue(inst) :
        mi is FieldInfo fi ? fi.GetValue(inst) :
        throw new InvalidOperationException("Member is not a property or field.");

    public static List<FieldInfo> GetSerializableFields(this Type t, BindingFlags flags = AllInstance)
    {
        List<FieldInfo> fields = new();
        while (t != null && t != typeof(object) && t != typeof(Object))
        {
            foreach (FieldInfo f in t.GetFields(flags))
                if (f.IsSerializableField())
                    fields.Add(f);

            t = t.BaseType;
        }

        return fields;
    }

    public static List<PropertyInfo> GetSerializableProperties(this Type t, BindingFlags flags = AllInstance)
    {
        List<PropertyInfo> props = new();
        while (t != null && t != typeof(object))
        {
            foreach (PropertyInfo p in t.GetProperties(flags))
                if (p.IsSerializableProperty())
                    props.Add(p);

            t = t.BaseType;
        }

        return props;
    }

    public static bool IsSerializable(this Type t) =>
        t.IsPrimitive ||
        t.IsEnum ||
        t == typeof(string) ||
        t == typeof(decimal) ||
        t.IsSerializable ||
        typeof(Object).IsAssignableFrom(t);

    public static bool IsSerializableField(this FieldInfo fi) =>
        (fi.IsPublic || fi.GetCustomAttribute<SerializeField>() != null) &&
        !ExcludedPropertyNames.Contains(fi.Name) &&
        !fi.IsInitOnly &&
        !fi.IsLiteral;

    public static bool IsSerializableProperty(this PropertyInfo pi) =>
        pi.CanRead &&
        pi.CanWrite &&
        !ExcludedPropertyNames.Contains(pi.Name) &&
        pi.PropertyType.IsSerializable() &&
        !pi.GetIndexParameters().Any();

    public static T GetAttribute<T>(this MemberInfo mi) where T : Attribute
        => mi.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;

    public static bool HasAttribute<T>(this MemberInfo mi) where T : Attribute
        =>
            mi.GetAttribute<T>() != null;

    public static IEnumerable<MethodInfo> GetMethodsWithAttribute<TAttribute>(this Type t, BindingFlags flags = All)
        where TAttribute : Attribute =>
        t.GetMethods(flags).Where(m => m.GetCustomAttributes(typeof(TAttribute), true).Any());

    public static Dictionary<MemberInfo, T> GetAttributesForMembers<T>(this Type t, BindingFlags flags = All)
        where T : Attribute =>
        t.GetMembers(flags).
            Where(m => m.GetCustomAttributes(typeof(T), true).Any()).
            ToDictionary(m => m, m => m.GetCustomAttribute<T>());

    public static Dictionary<MemberInfo, List<Attribute>> GetMembersWithAttributes
    (
        this Type t,
        BindingFlags flags = All, params Type[] attributeTypes
    ) =>
        t.GetMembers(flags).
            Select(member => new
            {
                member,
                attributes = attributeTypes.
                    SelectMany(attrType => member.GetCustomAttributes(attrType, true).Cast<Attribute>()).
                    ToList()
            }).
            Where(x => x.attributes.Any()).
            ToDictionary(x => x.member, x => x.attributes);

    public static bool IsList(this Type t) =>
        t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(List<>);

    public static bool IsNum(this Type t) =>
        !t.IsEnum && Type.GetTypeCode(t) is >= TypeCode.SByte and <= TypeCode.Decimal;

    public static bool IsArrayOf(this Type t, Type elementType)
        => t.IsArray && (t.GetElementType().IsSubclassOf(elementType) || t.GetElementType() == elementType);

    public static bool IsListOf(this Type t, Type elementType)
        => t.IsGenericType &&
           t.GetGenericTypeDefinition() == typeof(List<>) &&
           (t.GetGenericArguments()[0].IsSubclassOf(elementType) || t.GetGenericArguments()[0] == elementType);

    public static bool IsUnityCollection(this Type t) =>
        t.IsArray || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>));

    public static object ConvertToType(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return GetDefault(targetType);

        try
        {
            if (targetType == typeof(string)) return value;
            if (targetType == typeof(int)) return int.Parse(value);
            if (targetType == typeof(float)) return float.Parse(value);
            if (targetType == typeof(double)) return double.Parse(value);
            if (targetType == typeof(bool)) return bool.Parse(value);
            if (targetType == typeof(long)) return long.Parse(value);
            if (targetType == typeof(Vector3)) return ParseVector3(value);
            if (targetType.IsEnum) return Enum.Parse(targetType, value);

            // Handle nullable types
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (string.IsNullOrEmpty(value)) return null;
                targetType = Nullable.GetUnderlyingType(targetType);
            }
        }
        catch (Exception ex)
        {
            DLog.LogW($"Failed to convert '{value}' to type '{targetType.Name}': {ex.Message}");
            return GetDefault(targetType);
        }

        // Convert.ChangeType fallback for general conversion
        try { return Convert.ChangeType(value, targetType); }
        catch { return GetDefault(targetType); }
    }

    public static object GetDefault(Type targetType) =>
        targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

    //"(x, y, z)"
    private static Vector3 ParseVector3(string value)
    {
        value = value.Trim('(', ')');
        string[] parts = value.Split(',');
        if (parts.Length != 3) throw new FormatException("Invalid Vector3 format.");
        return new Vector3(
            float.Parse(parts[0]),
            float.Parse(parts[1]),
            float.Parse(parts[2])
        );
    }
}