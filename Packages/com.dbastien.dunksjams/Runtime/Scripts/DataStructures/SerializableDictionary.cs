using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
    Dictionary<TKey, TValue> _dict = new();

    [SerializeField] TKey[] _keys;
    [SerializeField] TValue[] _values;

    public TValue this[TKey key]
    {
        get => _dict[key];
        set => _dict[key] = value;
    }

    public int Count => _dict.Count;
    public void Add(TKey key, TValue val) => _dict.Add(key, val);
    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
    public bool Remove(TKey key) => _dict.Remove(key);
    public bool TryGetValue(TKey key, out TValue val) => _dict.TryGetValue(key, out val);

    public ICollection<TKey> Keys => _dict.Keys;
    public ICollection<TValue> Values => _dict.Values;

    public void OnBeforeSerialize()
    {
        _keys = new TKey[_dict.Count];
        _values = new TValue[_dict.Count];
        int i = 0;

        foreach (var kvp in _dict)
        {
            _keys[i] = kvp.Key;
            _values[i] = kvp.Value;
            ++i;
        }
    }

    public void OnAfterDeserialize()
    {
        _dict.Clear();

        if (_keys.Length != _values.Length)
        {
            DLog.LogW("Deserialization error: _keys and _values array lengths do not match.");
            return;
        }

        for (int i = 0; i < _keys.Length; ++i)
        {
            if (_dict.ContainsKey(_keys[i]))
            {
                DLog.LogW($"Duplicate key found during deserialization: {_keys[i]}. Skipping entry.");
                continue;
            }

            _dict[_keys[i]] = _values[i];
        }

        //keep the lists around so we can manipulate them in the editor from the property drawer
        //or uh yeah maybe just get rid of them
#if !UNITY_EDITOR
        _keys = null;
        _values = null;
#endif
    }
}