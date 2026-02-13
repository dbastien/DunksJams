using System;
using UnityEngine;

public class GameObjectUtils
{
    public static GameObject FindOrCreate(string name)
    {
        var obj = GameObject.Find(name);
        return obj ? obj : new GameObject(name);
    }

    public static GameObject FindOrCreate(string name, params Type[] components)
    {
        var obj = GameObject.Find(name);
        return obj ?? new GameObject(name, components);
    }
}