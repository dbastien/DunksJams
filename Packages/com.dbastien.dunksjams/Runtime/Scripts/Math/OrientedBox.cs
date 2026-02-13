using UnityEngine;

public class OrientedBox
{
    public Vector3 Size;
    public Vector3 Pos;
    Quaternion invRot;

    public Quaternion Rot { get; private set; }

    public OrientedBox(Vector3 size, Quaternion rot, Vector3 pos)
    {
        Size = size;
        Rot = rot;
        Pos = pos;
        invRot = Quaternion.Inverse(rot);
    }

    public bool Contains(Vector3 point, out Vector3 localPoint)
    {
        var newBox = new Bounds(Vector3.zero, Size);
        localPoint = invRot * (point - Pos);
        return newBox.Contains(localPoint);
    }

    // check if box B is fully contained in this
    public bool Contains(OrientedBox B)
    {
        var Points = new Vector3[8]
        {
            0.5f * new Vector3(B.Size.x, B.Size.y, B.Size.z),
            0.5f * new Vector3(B.Size.x, B.Size.y, -B.Size.z),
            0.5f * new Vector3(B.Size.x, -B.Size.y, B.Size.z),
            0.5f * new Vector3(B.Size.x, -B.Size.y, -B.Size.z),
            0.5f * new Vector3(-B.Size.x, B.Size.y, B.Size.z),
            0.5f * new Vector3(-B.Size.x, B.Size.y, -B.Size.z),
            0.5f * new Vector3(-B.Size.x, -B.Size.y, B.Size.z),
            0.5f * new Vector3(-B.Size.x, -B.Size.y, -B.Size.z)
        };

        for (var i = 0; i < 8; ++i)
        {
            var worldPt = B.TransformPointToWorld(Points[i]);
            Vector3 localVert;
            if (!Contains(worldPt, out localVert))
                return false;
        }

        return true;
    }

    public Vector3 TransformPointToWorld(Vector3 point) => Rot * point + Pos;
    public Vector3 TransformWorldPointToLocal(Vector3 point) => invRot * (point - Pos);
    public Vector3 TransformDirectionToWorld(Vector3 dir) => Rot * dir;
}