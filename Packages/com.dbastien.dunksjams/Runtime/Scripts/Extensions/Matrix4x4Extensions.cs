using System;
using UnityEngine;

public static class Matrix4X4Extensions
{
    public static Matrix4x4 CreateHSVTransform(float h, float s, float v)
    {
        float hr = -h * Mathf.Deg2Rad;

        float vsu = v * s * MathF.Cos(hr);
        float vsw = v * s * MathF.Sin(hr);

        return new Matrix4x4
        {
            m00 = .299f * v + .701f * vsu + .168f * vsw,
            m10 = .587f * v - .587f * vsu + .330f * vsw,
            m20 = .114f * v - .114f * vsu - .497f * vsw,

            m01 = .299f * v - .299f * vsu - .328f * vsw,
            m11 = .587f * v + .413f * vsu + .035f * vsw,
            m21 = .114f * v - .114f * vsu + .292f * vsw,

            m02 = .299f * v - .300f * vsu + 1.25f * vsw,
            m12 = .587f * v - .588f * vsu - 1.05f * vsw,
            m22 = .114f * v + .886f * vsu - .203f * vsw,

            m33 = 1f
        };
    }

    public static Matrix4x4 CreateHueRotationTransform(float degrees)
    {
        float angleRad = degrees * Mathf.Deg2Rad;
        float cosA = MathF.Cos(angleRad);
        float sinA = MathF.Sin(angleRad);

        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = 0.213f + cosA * 0.787f - sinA * 0.213f;
        m.m01 = 0.213f - cosA * 0.213f + sinA * 0.143f;
        m.m02 = 0.213f - cosA * 0.213f - sinA * 0.787f;

        m.m10 = 0.715f - cosA * 0.715f - sinA * 0.715f;
        m.m11 = 0.715f + cosA * 0.285f + sinA * 0.140f;
        m.m12 = 0.715f - cosA * 0.715f + sinA * 0.140f;

        m.m20 = 0.072f - cosA * 0.072f + sinA * 0.928f;
        m.m21 = 0.072f - cosA * 0.072f - sinA * 0.283f;
        m.m22 = 0.072f + cosA * 0.928f - sinA * 0.072f;

        m.m33 = 1f;
        return m;
    }

    public static Matrix4x4 CreateSaturationTransform(float sat)
    {
        Matrix4x4 m = Matrix4x4.identity;
        float invSat = 1f - sat;

        m.m00 = invSat * 0.299f + sat;
        m.m01 = invSat * 0.299f;
        m.m02 = invSat * 0.299f;

        m.m10 = invSat * 0.587f;
        m.m11 = invSat * 0.587f + sat;
        m.m12 = invSat * 0.587f;

        m.m20 = invSat * 0.114f;
        m.m21 = invSat * 0.114f;
        m.m22 = invSat * 0.114f + sat;

        m.m33 = 1f;
        return m;
    }

    public static Matrix4x4 CreateGrayscaleTransform()
    {
        Matrix4x4 m = default;
        m.m00 = m.m01 = m.m02 = 0.299f;
        m.m10 = m.m11 = m.m12 = 0.587f;
        m.m20 = m.m21 = m.m22 = 0.114f;
        m.m33 = 1f;
        return m;
    }

    public static Matrix4x4 CreateSepiaFilter()
    {
        return new Matrix4x4
        {
            m00 = 0.393f, m01 = 0.349f, m02 = 0.272f,
            m10 = 0.769f, m11 = 0.686f, m12 = 0.534f,
            m20 = 0.189f, m21 = 0.168f, m22 = 0.131f,
            m33 = 1f
        };
    }

    public static Vector2 TransformVector2D(this Matrix4x4 m, Vector2 v)
    {
        Vector3 r = m.MultiplyVector(new Vector3(v.x, v.y, 0f));
        return new Vector2(r.x, r.y);
    }

    public static Vector2 TransformPoint2D(this Matrix4x4 m, Vector2 v)
    {
        Vector3 r = m.MultiplyPoint3x4(new Vector3(v.x, v.y, 0f));
        return new Vector2(r.x, r.y);
    }

    public static Matrix4x4 CreateScaleRotationTranslation2D(Vector2 scale, float radians, Vector2 translation)
    {
        float c = MathF.Cos(radians);
        float s = MathF.Sin(radians);

        Matrix4x4 m = Matrix4x4.identity;

        m.m00 = c * scale.x;
        m.m10 = s * scale.x;

        m.m01 = -s * scale.y;
        m.m11 = c * scale.y;

        m.m03 = translation.x;
        m.m13 = translation.y;

        m.m22 = 1f;
        m.m33 = 1f;

        return m;
    }
}