using System;
using UnityEngine;

public class GameObjectUtils
{
    public static GameObject FindOrCreate(string name)
    {
        GameObject obj = GameObject.Find(name);
        return obj ? obj : new(name);
    }
    
    public static GameObject FindOrCreate(string name, params Type[] components)
    {
        GameObject obj = GameObject.Find(name);
        return obj ?? new GameObject(name, components);
    }
}