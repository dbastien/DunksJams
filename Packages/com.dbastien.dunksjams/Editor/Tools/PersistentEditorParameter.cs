#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public enum EditorValueStorage
    {
        /// <summary>Persists across editor restarts (per user / machine).</summary>
        Prefs,

        /// <summary>Persists only for the current Unity session (survives domain reloads).</summary>
        Session
    }

    /// <summary>Cached editor value stored by key in either EditorPrefs (persistent) or SessionState (session-only). Supports bool/int/float/string, enums, and JSON fallback for Unity-serializable types.</summary>
    public sealed class EditorStoredValue<T>
    {
        readonly string _key;
        readonly T _defaultValue;
        readonly EditorValueStorage _storage;

        bool _loaded;
        T _value;

        public string Key => _key;
        public EditorValueStorage Storage => _storage;

        public EditorStoredValue(
            string key,
            T defaultValue = default,
            EditorValueStorage storage = EditorValueStorage.Session,
            bool projectScoped = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must be non-empty.", nameof(key));

            _defaultValue = defaultValue;
            _value = defaultValue;
            _storage = storage;
            _loaded = false;

            _key = projectScoped ? $"{ProjectScopePrefix}:{key}" : key;
        }

        public T Value
        {
            get
            {
                EnsureLoaded();
                return _value;
            }
            set
            {
                EnsureLoaded();
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                Save(value);
            }
        }

        public void Reset() => Value = _defaultValue;

        /// <summary>Forces reload from storage next time Value is read.</summary>
        public void InvalidateCache() => _loaded = false;

        /// <summary>Deletes/erases the stored key (and resets the cached value to default).</summary>
        public void Clear()
        {
            Erase();
            _value = _defaultValue;
            _loaded = true;
        }

        void EnsureLoaded()
        {
            if (_loaded) return;
            _loaded = true;
            _value = Load();
        }

        T Load()
        {
            var t = typeof(T);

            if (t == typeof(bool))
                return (T)(object)GetBool(_key, AsBool(_defaultValue));

            if (t == typeof(int))
                return (T)(object)GetInt(_key, AsInt(_defaultValue));

            if (t == typeof(float))
                return (T)(object)GetFloat(_key, AsFloat(_defaultValue));

            if (t == typeof(string))
                return (T)(object)GetString(_key, _defaultValue as string);

            if (t.IsEnum)
            {
                var def = Convert.ToInt32(_defaultValue);
                var v = GetInt(_key, def);
                return (T)Enum.ToObject(t, v);
            }

            // JSON fallback (Unity-serializable types)
            var json = GetString(_key, null);
            if (string.IsNullOrEmpty(json))
                return _defaultValue;

            try
            {
                var box = JsonUtility.FromJson<Box>(json);
                return box != null ? box.value : _defaultValue;
            }
            catch
            {
                return _defaultValue;
            }
        }

        void Save(T value)
        {
            var t = typeof(T);

            if (t == typeof(bool))
            {
                SetBool(_key, (bool)(object)value);
                return;
            }

            if (t == typeof(int))
            {
                SetInt(_key, (int)(object)value);
                return;
            }

            if (t == typeof(float))
            {
                SetFloat(_key, (float)(object)value);
                return;
            }

            if (t == typeof(string))
            {
                SetString(_key, (string)(object)value);
                return;
            }

            if (t.IsEnum)
            {
                SetInt(_key, Convert.ToInt32(value));
                return;
            }

            // JSON fallback
            var json = JsonUtility.ToJson(new Box { value = value });
            SetString(_key, json);
        }

        void Erase()
        {
            var t = typeof(T);

            if (t == typeof(bool))
            {
                EraseBool(_key);
                return;
            }

            if (t == typeof(int))
            {
                EraseInt(_key);
                return;
            }

            if (t == typeof(float))
            {
                EraseFloat(_key);
                return;
            }

            if (t == typeof(string))
            {
                EraseString(_key);
                return;
            }

            if (t.IsEnum)
            {
                EraseInt(_key);
                return;
            }

            // JSON fallback stored as string
            EraseString(_key);
        }

        [Serializable]
        sealed class Box
        {
            public T value;
        }

        static readonly string ProjectScopePrefix =
            $"Project[{Hash128.Compute(Application.dataPath ?? "unknown_project")}]";

        bool GetBool(string key, bool def) =>
            _storage == EditorValueStorage.Prefs ? EditorPrefs.GetBool(key, def) : SessionState.GetBool(key, def);

        int GetInt(string key, int def) =>
            _storage == EditorValueStorage.Prefs ? EditorPrefs.GetInt(key, def) : SessionState.GetInt(key, def);

        float GetFloat(string key, float def) =>
            _storage == EditorValueStorage.Prefs ? EditorPrefs.GetFloat(key, def) : SessionState.GetFloat(key, def);

        string GetString(string key, string def) =>
            _storage == EditorValueStorage.Prefs ? EditorPrefs.GetString(key, def) : SessionState.GetString(key, def);

        void SetBool(string key, bool v)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetBool(key, v);
            else SessionState.SetBool(key, v);
        }

        void SetInt(string key, int v)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetInt(key, v);
            else SessionState.SetInt(key, v);
        }

        void SetFloat(string key, float v)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetFloat(key, v);
            else SessionState.SetFloat(key, v);
        }

        void SetString(string key, string v)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetString(key, v ?? string.Empty);
            else SessionState.SetString(key, v ?? string.Empty);
        }

        void EraseBool(string key)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
            else SessionState.EraseBool(key);
        }

        void EraseInt(string key)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
            else SessionState.EraseInt(key);
        }

        void EraseFloat(string key)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
            else SessionState.EraseFloat(key);
        }

        void EraseString(string key)
        {
            if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
            else SessionState.EraseString(key);
        }

        static bool AsBool(T v) => v is bool b ? b : default;
        static int AsInt(T v) => v is int i ? i : default;
        static float AsFloat(T v) => v is float f ? f : default;
    }
#endif