using System;
using UnityEngine;

public class GameObjectUtils
{
    public static GameObject FindOrCreate(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj ? obj : new GameObject(name);
    }

    public static GameObject FindOrCreate(string name, params Type[] components)
    {
        GameObject obj = GameObject.Find(name);
        return obj ?? new GameObject(name, components);
    }

    public static GameObject Find(string nameOrTag)
    {
        if (string.IsNullOrEmpty(nameOrTag)) return null;
        GameObject obj = GameObject.Find(nameOrTag);
        if (obj) return obj;
        try { return GameObject.FindWithTag(nameOrTag); }
        catch { return null; }
    }
}