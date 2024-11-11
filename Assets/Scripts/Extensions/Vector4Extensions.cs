using UnityEngine;

//todo: add "ref this" as well as "this" methods

public static class Vector4Extensions
{
    public static Vector4 LerpUnclamped(this Vector4 l, Vector4 r, float t) => l + t * (r - l);

    public static Vector4 Clamp(this Vector4 v, Vector4 min, Vector4 max) =>
        new(Mathf.Clamp(v.x, min.x, max.x),
            Mathf.Clamp(v.y, min.y, max.y),
            Mathf.Clamp(v.z, min.z, max.z),
            Mathf.Clamp(v.w, min.w, max.w));
    
    public static Vector4 Scaled(this Vector4 v, Vector4 scale) => 
        new(v.x * scale.x, v.y * scale.y, v.z * scale.z, v.w * scale.w);
}
