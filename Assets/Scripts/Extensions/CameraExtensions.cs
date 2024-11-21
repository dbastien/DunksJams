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
        System.IO.File.WriteAllBytes(path, bytes);
        DLog.Log($"Screenshot saved to: {path}");
        Object.Destroy(screenShot);
    }
    
    public static void SetOrthographic(this Camera cam)
    {
        cam.orthographic = true;
        cam.ResetProjectionMatrix();
        cam.transform.rotation = Quaternion.identity;
    }

    public static void SetOblique(this Camera cam, float angle, float scale = 1f)
    {
        cam.orthographic = true;
        float rad = angle * Mathf.Deg2Rad;

        Matrix4x4 mat = cam.projectionMatrix;
        mat.m02 = scale * Mathf.Cos(rad);
        mat.m12 = scale * Mathf.Sin(rad);

        cam.projectionMatrix = mat;
    }

    public static void SetAxonometricProjection(this Camera cam, float xAngle, float yAngle, float zAngle = 0)
    {
        cam.orthographic = true;
        cam.ResetProjectionMatrix();
        cam.transform.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

    public static void SetPerspective(this Camera cam, float xAngle = 0, float yAngle = 0, float zAngle = 0)
    {
        cam.orthographic = false;
        cam.fieldOfView = 60f;
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