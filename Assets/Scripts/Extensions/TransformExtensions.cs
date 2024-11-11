using System;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TransformExtensions
{
    public static void SetPositionX(this Transform t, float x) => t.position = new(x, t.position.y, t.position.z);
    public static void SetPositionY(this Transform t, float y) => t.position = new(t.position.x, y, t.position.z);
    public static void SetPositionZ(this Transform t, float z) => t.position = new(t.position.x, t.position.y, z);
    
    public static void SetLocalPositionX(this Transform t, float x) => t.localPosition = new(x, t.localPosition.y, t.localPosition.z);
    public static void SetLocalPositionY(this Transform t, float y) => t.localPosition = new(t.localPosition.x, y, t.localPosition.z);
    public static void SetLocalPositionZ(this Transform t, float z) => t.localPosition = new(t.localPosition.x, t.localPosition.y, z);
    
    public static void SetScaleX(this Transform t, float x) => t.localScale = new(x, t.localScale.y, t.localScale.z);
    public static void SetScaleY(this Transform t, float y) => t.localScale = new(t.localScale.x, y, t.localScale.z);
    public static void SetScaleZ(this Transform t, float z) => t.localScale = new(t.localScale.x, t.localScale.y, z);
    
    public static void FlipX(this Transform t) => t.localScale = new(-t.localScale.x, t.localScale.y, t.localScale.z);
    public static void FlipY(this Transform t) => t.localScale = new(t.localScale.x, -t.localScale.y, t.localScale.z);
    public static void FlipZ(this Transform t) => t.localScale = new(t.localScale.x, t.localScale.y, -t.localScale.z);
    
    public static void ResetTransform(this Transform t)
    {
        t.position = Vector3.zero;
        t.rotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }
    
    public static void CopyTransformFrom(this Transform t, Transform source)
    {
        t.SetPositionAndRotation(source.position, source.rotation);
        t.localScale = source.localScale;
    }
    
    public static void CopyTransformTo(this Transform t, Transform dest)
    {
        dest.SetPositionAndRotation(t.position, t.rotation);
        dest.localScale = t.localScale;
    }
    
    public static void LookAt2D(this Transform t, Vector3 target)
    {
        Vector3 dir = target - t.position;
        t.rotation = Quaternion.LookRotation(Vector3.forward, dir);
    }
    
    public static void LookAt2D(this Transform t, Vector2 target)
    {
        Vector3 dir = target - (Vector2)t.position;
        t.rotation = Quaternion.Euler(0, 0, MathF.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
    }
    
    public static Transform FindOrCreateChild(this Transform t, string name)
    {
        Transform child = t.Find(name);
        if (child) return child;

        var newChild = new GameObject(name).transform;
        newChild.SetParent(t);
        newChild.ResetTransform();
        return newChild;
    }
    
    public static void DestroyChildren(this Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; --i)
        {
            var child = t.GetChild(i).gameObject;
            if (Application.isPlaying)
                Object.Destroy(child);
            else
                Object.DestroyImmediate(child);
        }
        t.DetachChildren();
    }
    
    public static string GetPath(this Transform t)
    {
        string path = t.name;
        while (t.parent)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
    
    public static Vector3 ToScreenSpace(this Transform t, Camera cam) =>
        cam.WorldToScreenPoint(t.position);

    public static void AlignTo(this Transform t, Transform target) =>
        t.rotation = target.rotation;
}