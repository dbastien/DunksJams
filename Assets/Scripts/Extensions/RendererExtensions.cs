using UnityEngine;

public static class RendererExtensions
{
    public static Rect GetScreenBounds(this Renderer r, Camera cam)
    {
        Bounds bounds = r.bounds;
        Vector3 min = cam.WorldToScreenPoint(bounds.min);
        Vector3 max = cam.WorldToScreenPoint(bounds.max);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
    
    public static bool IsVisibleFrom(this Renderer r, Camera cam) => 
        GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam), r.bounds);
    
    public static bool IsFullyVisibleFrom(this Renderer r, Camera cam)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        foreach (Plane plane in planes)
        {
            if (!(plane.GetDistanceToPoint(r.bounds.center) >= -r.bounds.extents.magnitude)) 
                return false;
        }

        return true;
    }
    
    public static void ReplaceMaterial(this Renderer r, Material mat)
    {
        Material[] materials = r.materials;
        for (var i = 0; i < materials.Length; ++i) materials[i] = mat;
        r.materials = materials;
    }
}