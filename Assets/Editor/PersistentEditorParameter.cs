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
	Session,
}

/// <summary>
/// Cached editor value stored by key in either EditorPrefs (persistent) or SessionState (session-only).
/// Supports bool/int/float/string, enums, and JSON fallback for Unity-serializable types.
/// </summary>
public sealed class EditorStoredValue<T>
{
	private readonly string _key;
	private readonly T _defaultValue;
	private readonly EditorValueStorage _storage;

	private bool _loaded;
	private T _value;

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

	private void EnsureLoaded()
	{
		if (_loaded) return;
		_loaded = true;
		_value = Load();
	}

	private T Load()
	{
		Type t = typeof(T);

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
			int def = Convert.ToInt32(_defaultValue);
			int v = GetInt(_key, def);
			return (T)Enum.ToObject(t, v);
		}

		// JSON fallback (Unity-serializable types)
		string json = GetString(_key, null);
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

	private void Save(T value)
	{
		Type t = typeof(T);

		if (t == typeof(bool))  { SetBool(_key, (bool)(object)value); return; }
		if (t == typeof(int))   { SetInt(_key, (int)(object)value); return; }
		if (t == typeof(float)) { SetFloat(_key, (float)(object)value); return; }
		if (t == typeof(string)){ SetString(_key, (string)(object)value); return; }

		if (t.IsEnum)
		{
			SetInt(_key, Convert.ToInt32(value));
			return;
		}

		// JSON fallback
		string json = JsonUtility.ToJson(new Box { value = value });
		SetString(_key, json);
	}

	private void Erase()
	{
		Type t = typeof(T);

		if (t == typeof(bool))  { EraseBool(_key); return; }
		if (t == typeof(int))   { EraseInt(_key); return; }
		if (t == typeof(float)) { EraseFloat(_key); return; }
		if (t == typeof(string)){ EraseString(_key); return; }
		if (t.IsEnum)           { EraseInt(_key); return; }

		// JSON fallback stored as string
		EraseString(_key);
	}

	[Serializable]
	private sealed class Box
	{
		public T value;
	}

	private static readonly string ProjectScopePrefix =
		$"Project[{Hash128.Compute(Application.dataPath ?? "unknown_project")}]";

	private bool GetBool(string key, bool def) =>
		_storage == EditorValueStorage.Prefs ? EditorPrefs.GetBool(key, def) : SessionState.GetBool(key, def);

	private int GetInt(string key, int def) =>
		_storage == EditorValueStorage.Prefs ? EditorPrefs.GetInt(key, def) : SessionState.GetInt(key, def);

	private float GetFloat(string key, float def) =>
		_storage == EditorValueStorage.Prefs ? EditorPrefs.GetFloat(key, def) : SessionState.GetFloat(key, def);

	private string GetString(string key, string def) =>
		_storage == EditorValueStorage.Prefs ? EditorPrefs.GetString(key, def) : SessionState.GetString(key, def);

	private void SetBool(string key, bool v)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetBool(key, v);
		else SessionState.SetBool(key, v);
	}

	private void SetInt(string key, int v)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetInt(key, v);
		else SessionState.SetInt(key, v);
	}

	private void SetFloat(string key, float v)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetFloat(key, v);
		else SessionState.SetFloat(key, v);
	}

	private void SetString(string key, string v)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.SetString(key, v ?? string.Empty);
		else SessionState.SetString(key, v ?? string.Empty);
	}

	private void EraseBool(string key)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
		else SessionState.EraseBool(key);
	}

	private void EraseInt(string key)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
		else SessionState.EraseInt(key);
	}

	private void EraseFloat(string key)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
		else SessionState.EraseFloat(key);
	}

	private void EraseString(string key)
	{
		if (_storage == EditorValueStorage.Prefs) EditorPrefs.DeleteKey(key);
		else SessionState.EraseString(key);
	}

	private static bool AsBool(T v) => v is bool b ? b : default;
	private static int AsInt(T v) => v is int i ? i : default;
	private static float AsFloat(T v) => v is float f ? f : default;
}
#endif
