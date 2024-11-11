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
}