#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EditorValueStorage { Prefs, Session }

public sealed class EditorStoredValue<T>
{
    readonly string key;
    readonly T defaultValue;
    readonly Backend backend;

    bool loaded;
    T value;

    public string Key => key;
    public EditorValueStorage Storage { get; }

    public EditorStoredValue(string key, T defaultValue = default, EditorValueStorage storage = EditorValueStorage.Session, bool projectScoped = true)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key must be non-empty.", nameof(key));

        Storage = storage;
        backend = Backend.For(storage);

        this.defaultValue = defaultValue;
        value = defaultValue;
        loaded = false;

        this.key = projectScoped ? $"{ProjectScopePrefix}:{key}" : key;
    }

    public T Value
    {
        get { EnsureLoaded(); return value; }
        set
        {
            EnsureLoaded();
            if (EqualityComparer<T>.Default.Equals(this.value, value)) return;
            this.value = value;
            Save(value);
        }
    }

    public void Reset() => Value = defaultValue;
    public void InvalidateCache() => loaded = false;

    public void Clear()
    {
        Erase();
        value = defaultValue;
        loaded = true;
    }

    void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;
        value = Load();
    }

    T Load()
    {
        var t = typeof(T);

        if (t == typeof(bool))   return (T)(object)backend.GetBool(key, defaultValue is bool b ? b : default);
        if (t == typeof(int))    return (T)(object)backend.GetInt(key, defaultValue is int i ? i : default);
        if (t == typeof(float))  return (T)(object)backend.GetFloat(key, defaultValue is float f ? f : default);
        if (t == typeof(string)) return (T)(object)backend.GetString(key, defaultValue as string);

        if (t.IsEnum)
        {
            int def = Convert.ToInt32(defaultValue);
            int v = backend.GetInt(key, def);
            return (T)Enum.ToObject(t, v);
        }

        var json = backend.GetString(key, null);
        if (string.IsNullOrEmpty(json)) return defaultValue;

        try
        {
            var box = JsonUtility.FromJson<Box>(json);
            return box != null ? box.value : defaultValue;
        }
        catch { return defaultValue; }
    }

    void Save(T v)
    {
        var t = typeof(T);

        if (t == typeof(bool))   { backend.SetBool(key, (bool)(object)v); return; }
        if (t == typeof(int))    { backend.SetInt(key, (int)(object)v); return; }
        if (t == typeof(float))  { backend.SetFloat(key, (float)(object)v); return; }
        if (t == typeof(string)) { backend.SetString(key, (string)(object)v); return; }

        if (t.IsEnum) { backend.SetInt(key, Convert.ToInt32(v)); return; }

        backend.SetString(key, JsonUtility.ToJson(new Box { value = v }));
    }

    void Erase()
    {
        var t = typeof(T);

        if (t == typeof(bool))   { backend.DeleteKeyOrEraseBool(key); return; }
        if (t == typeof(int))    { backend.DeleteKeyOrEraseInt(key); return; }
        if (t == typeof(float))  { backend.DeleteKeyOrEraseFloat(key); return; }
        // string, enum, json fallback all stored as string or prefs key
        backend.DeleteKeyOrEraseString(key);
    }

    [Serializable] sealed class Box { public T value; }

    static readonly string ProjectScopePrefix =
        $"Project[{Hash128.Compute(Application.dataPath ?? "unknown_project")}]";

    readonly struct Backend
    {
        public readonly Func<string, bool, bool> GetBool;
        public readonly Func<string, int, int> GetInt;
        public readonly Func<string, float, float> GetFloat;
        public readonly Func<string, string, string> GetString;

        public readonly Action<string, bool> SetBool;
        public readonly Action<string, int> SetInt;
        public readonly Action<string, float> SetFloat;
        public readonly Action<string, string> SetString;

        public readonly Action<string> DeleteKeyOrEraseBool;
        public readonly Action<string> DeleteKeyOrEraseInt;
        public readonly Action<string> DeleteKeyOrEraseFloat;
        public readonly Action<string> DeleteKeyOrEraseString;

        Backend(
            Func<string, bool, bool> getBool,
            Func<string, int, int> getInt,
            Func<string, float, float> getFloat,
            Func<string, string, string> getString,
            Action<string, bool> setBool,
            Action<string, int> setInt,
            Action<string, float> setFloat,
            Action<string, string> setString,
            Action<string> delBool,
            Action<string> delInt,
            Action<string> delFloat,
            Action<string> delString)
        {
            GetBool = getBool; GetInt = getInt; GetFloat = getFloat; GetString = getString;
            SetBool = setBool; SetInt = setInt; SetFloat = setFloat; SetString = setString;
            DeleteKeyOrEraseBool = delBool; DeleteKeyOrEraseInt = delInt; DeleteKeyOrEraseFloat = delFloat; DeleteKeyOrEraseString = delString;
        }

        public static Backend For(EditorValueStorage storage)
        {
            if (storage == EditorValueStorage.Prefs)
            {
                return new Backend(
                    EditorPrefs.GetBool,
                    EditorPrefs.GetInt,
                    EditorPrefs.GetFloat,
                    EditorPrefs.GetString,
                    EditorPrefs.SetBool,
                    EditorPrefs.SetInt,
                    EditorPrefs.SetFloat,
                    (k, v) => EditorPrefs.SetString(k, v ?? string.Empty),
                    k => EditorPrefs.DeleteKey(k),
                    k => EditorPrefs.DeleteKey(k),
                    k => EditorPrefs.DeleteKey(k),
                    k => EditorPrefs.DeleteKey(k)
                );
            }

            return new Backend(
                SessionState.GetBool,
                SessionState.GetInt,
                SessionState.GetFloat,
                SessionState.GetString,
                SessionState.SetBool,
                SessionState.SetInt,
                SessionState.SetFloat,
                (k, v) => SessionState.SetString(k, v ?? string.Empty),
                SessionState.EraseBool,
                SessionState.EraseInt,
                SessionState.EraseFloat,
                SessionState.EraseString
            );
        }
    }
}
#endif