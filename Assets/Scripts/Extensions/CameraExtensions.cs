using System.IO;
using UnityEngine;

public static class CameraExtensions
{
    public static void SaveScreenshot(this Camera c, string path, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height, 24);
        c.targetTexture = rt;
        Texture2D screenShot = new(width, height, TextureFormat.RGB24, false);

        c.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new(0, 0, width, height), 0, 0);
        
        c.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        DLog.Log($"Screenshot saved to: {path}");
        Object.Destroy(screenShot);
    }
    
    public static void SetOrthographic(this Camera cam, bool resetRotation = false)
    {
        cam.orthographic = true;
        cam.ResetProjectionMatrix();

        if (resetRotation)
            cam.transform.rotation = Quaternion.identity;
    }

    public static void SetOblique(this Camera cam, float angleDeg, float scale = 1f)
    {
        cam.orthographic = true;

        // Critical: start from a clean orthographic projection matrix
        cam.ResetProjectionMatrix();
        var mat = cam.projectionMatrix;

        float rad = angleDeg * Mathf.Deg2Rad;

        // Shear Z as a function of X/Y (oblique projection)
        mat.m02 = scale * Mathf.Cos(rad);
        mat.m12 = scale * Mathf.Sin(rad);

        cam.projectionMatrix = mat;
    }

    public static void SetAxonometricProjection(this Camera cam, float xAngle, float yAngle, float zAngle = 0f, bool setRotation = true)
    {
        cam.orthographic = true;
        cam.ResetProjectionMatrix();

        if (setRotation)
            cam.transform.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

    public static void SetPerspective(this Camera cam, float xAngle = 0f, float yAngle = 0f, float zAngle = 0f,
        bool setRotation = true, float? fov = null)
    {
        cam.orthographic = false;

        // Critical: throw away any custom matrix from oblique etc.
        cam.ResetProjectionMatrix();

        if (fov.HasValue)
            cam.fieldOfView = fov.Value;

        if (setRotation)
            cam.transform.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

    public static void SetupDepth(this Camera cam,
        float apexOffsetZ, float maxDist, float radiusStart, float radiusEnd,
        Vector3 fwd, Vector3 scale, bool scalable, Quaternion localRot, bool scaleNearClip)
    {
        if (!scalable) scale = new(1f, 1f, scale.z);
        bool isCone = apexOffsetZ >= 0f;
        float offset = Mathf.Max(0f, apexOffsetZ);

        cam.orthographic = !isCone;
        cam.transform.localPosition = fwd * -offset;

        var rot = localRot;
        if (scale.z < 0f) rot *= Quaternion.Euler(0f, 180f, 0f);
        cam.transform.localRotation = rot;

        if (Mathf.Approximately(scale.y * scale.z, 0f)) return;

        float nearScale = scaleNearClip ? scale.z : 1f;
        cam.nearClipPlane = Mathf.Max(offset * scale.z, (isCone ? 0.1f : 0f) * nearScale);

        float farScale = scalable ? scale.z : 1f;
        cam.farClipPlane = maxDist + offset * farScale;

        cam.aspect = Mathf.Abs(scale.x / scale.y);

        if (isCone)
            cam.fieldOfView = 2f * Mathf.Rad2Deg * Mathf.Atan2(radiusEnd * scale.y, cam.farClipPlane);
        else
            cam.orthographicSize = radiusStart * scale.y;
    }
}