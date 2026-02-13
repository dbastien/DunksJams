using System;
using UnityEngine;

public struct Matrix3x2
{
    public float m00, m01; // X scale/rotation
    public float m10, m11; // Y scale/rotation
    public float m20, m21; // Translation (affine)

    public Matrix3x2(float m00, float m01, float m10, float m11, float m20, float m21)
    {
        this.m00 = m00;
        this.m01 = m01;
        this.m10 = m10;
        this.m11 = m11;
        this.m20 = m20;
        this.m21 = m21;
    }

    public float determinant => m00 * m11 - m01 * m10;
    public static Matrix3x2 identity => new(1, 0, 0, 1, 0, 0);

    public static Matrix3x2 operator *(Matrix3x2 a, Matrix3x2 b) => new(
        a.m00 * b.m00 + a.m01 * b.m10, a.m00 * b.m01 + a.m01 * b.m11,
        a.m10 * b.m00 + a.m11 * b.m10, a.m10 * b.m01 + a.m11 * b.m11,
        a.m20 * b.m00 + a.m21 * b.m10 + b.m20, a.m20 * b.m01 + a.m21 * b.m11 + b.m21
    );

    public Vector2 MultiplyPoint(Vector2 point) =>
        new(point.x * m00 + point.y * m10 + m20,
            point.x * m01 + point.y * m11 + m21);

    public Vector2 MultiplyVector(Vector2 vector) =>
        new(vector.x * m00 + vector.y * m10,
            vector.x * m01 + vector.y * m11);

    public Rect TransformRect(Rect rect)
    {
        var min = MultiplyPoint(rect.min);
        var max = MultiplyPoint(rect.max);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public Matrix3x2 inverse
    {
        get
        {
            var det = determinant;
            if (Mathf.Abs(det) < Mathf.Epsilon) throw new InvalidOperationException("Matrix is not invertible.");

            var invDet = 1.0f / det;
            return new Matrix3x2(
                m11 * invDet, -m01 * invDet,
                -m10 * invDet, m00 * invDet,
                (m10 * m21 - m11 * m20) * invDet,
                (m01 * m20 - m00 * m21) * invDet
            );
        }
    }

    public static Matrix3x2 Translation(Vector2 v) => new(1, 0, 0, 1, v.x, v.y);
    public static Matrix3x2 Scale(Vector2 s) => new(s.x, 0, 0, s.y, 0, 0);

    public static Matrix3x2 Rotation(float angle)
    {
        var rad = angle * Mathf.Deg2Rad;
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);
        return new Matrix3x2(cos, -sin, sin, cos, 0, 0);
    }

    public void Translate(Vector2 offset)
    {
        m20 += offset.x;
        m21 += offset.y;
    }

    public void Rotate(float angle)
    {
        var rad = angle * Mathf.Deg2Rad;
        var cos = Mathf.Cos(rad);
        var sin = Mathf.Sin(rad);

        var newM00 = m00 * cos - m01 * sin;
        var newM01 = m00 * sin + m01 * cos;
        var newM10 = m10 * cos - m11 * sin;
        var newM11 = m10 * sin + m11 * cos;

        m00 = newM00;
        m01 = newM01;
        m10 = newM10;
        m11 = newM11;
    }

    public override string ToString() =>
        $"|{m00:F2}, {m01:F2}|\n|{m10:F2}, {m11:F2}|\n|{m20:F2}, {m21:F2}|";
}