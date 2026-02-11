using System;
using UnityEditor;
using UnityEngine;

public static class AdvancedMeshMenu
{
    public enum MeshType { TriangularPrism, HexagonalPrism, Pyramid, Grid, Helix, Torus, GeodesicDome }

    [MenuItem("‽GameObject/3D Object/Triangular Prism")] public static void CreateTriangularPrism() => CreateMesh(MeshType.TriangularPrism);
    [MenuItem("‽GameObject/3D Object/Hexagonal Prism")] public static void CreateHexagonalPrism() => CreateMesh(MeshType.HexagonalPrism);
    [MenuItem("‽GameObject/3D Object/Pyramid")] public static void CreatePyramid() => CreateMesh(MeshType.Pyramid);
    [MenuItem("‽GameObject/3D Object/Grid")] public static void CreateGrid() => CreateMesh(MeshType.Grid);
    [MenuItem("‽GameObject/3D Object/Helix")] public static void CreateHelix() => CreateMesh(MeshType.Helix);
    [MenuItem("‽GameObject/3D Object/Torus")] public static void CreateTorus() => CreateMesh(MeshType.Torus);
    [MenuItem("‽GameObject/3D Object/Geodesic Dome")] public static void CreateGeodesicDome() => CreateMesh(MeshType.GeodesicDome);

    static void CreateMesh(MeshType type)
    {
        var go = new GameObject(type.ToString()) { name = type.ToString() };
        var meshFilter = go.AddComponent<MeshFilter>();
        var meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = DefaultResources.Material;

        meshFilter.mesh = GenerateMesh(type);
        Selection.activeObject = go;
    }

    static Mesh GenerateMesh(MeshType type)
    {
        float size = 1f; int resolution = 10;
        return type switch
        {
            MeshType.TriangularPrism => CreatePrism(3, size),
            MeshType.HexagonalPrism => CreatePrism(6, size),
            MeshType.Pyramid => CreatePyramid(size),
            MeshType.Grid => CreateGrid(size, resolution),
            MeshType.Helix => CreateHelix(size, resolution),
            MeshType.Torus => CreateTorus(size, resolution),
            MeshType.GeodesicDome => CreateGeodesicDome(size, resolution),
            _ => null
        };
    }

    static Mesh CreatePrism(int sides, float size)
    {
        Mesh mesh = new();
        var verts = new Vector3[sides * 2];
        var tris = new int[sides * 12];
        float angleStep = 2 * MathF.PI / sides;

        for (int i = 0; i < sides; ++i)
        {
            float angle = i * angleStep;
            verts[i] = new(MathF.Cos(angle) * size, 0, MathF.Sin(angle) * size);
            verts[i + sides] = verts[i] + Vector3.up * size;
        }

        for (int i = 0; i < sides; ++i)
        {
            int next = (i + 1) % sides;
            int t = i * 12;

            // Bottom and top faces
            AddTriangle(tris, t, 0, i, next);
            AddTriangle(tris, t + 3, sides, sides + next, sides + i);

            // Side faces
            AddQuad(tris, t + 6, i, next, sides + i, sides + next);
        }

        return SetupMesh(mesh, verts, tris);
    }

    static Mesh CreatePyramid(float size)
    {
        var verts = new[] {
            new Vector3(-size, 0, -size), new Vector3(size, 0, -size), new Vector3(size, 0, size), new Vector3(-size, 0, size), new Vector3(0, size, 0)
        };
        var tris = new[] { 0, 1, 4, 1, 2, 4, 2, 3, 4, 3, 0, 4, 0, 2, 1, 0, 3, 2 };
        return SetupMesh(new(), verts, tris);
    }

    static Mesh CreateGrid(float size, int res)
    {
        var verts = new Vector3[(res + 1) * (res + 1)];
        var tris = new int[res * res * 6];
        float step = size / res;

        for (int i = 0, y = 0; y <= res; ++y)
            for (int x = 0; x <= res; ++x, ++i)
                verts[i] = new(x * step, 0, y * step);

        for (int ti = 0, vi = 0, y = 0; y < res; ++y, ++vi)
            for (int x = 0; x < res; ++x, ti += 6, ++vi)
                AddQuad(tris, ti, vi, vi + 1, vi + res + 1, vi + res + 2);

        return SetupMesh(new(), verts, tris);
    }

    static Mesh CreateHelix(float size, int res)
    {
        var verts = new Vector3[res * 2];
        var tris = new int[(res - 1) * 6];
        float angleStep = 360f / res;
        float heightStep = size / res;

        for (int i = 0; i < res; ++i)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            verts[i] = new(MathF.Cos(angle), i * heightStep, MathF.Sin(angle));
            verts[i + res] = new(MathF.Cos(angle), (i + 1) * heightStep, MathF.Sin(angle));
        }

        for (int i = 0, ti = 0; i < res - 1; ++i, ti += 6)
            AddQuad(tris, ti, i, i + 1, i + res, i + res + 1);

        return SetupMesh(new(), verts, tris);
    }

    static Mesh CreateTorus(float size, int res)
    {
        var verts = new Vector3[res * res];
        var tris = new int[res * res * 6];
        float ringRadius = size / 4;
        float tubeRadius = size / 8;
        float ringStep = 360f / res;
        float tubeStep = 360f / res;

        for (int ring = 0; ring < res; ++ring)
        {
            float ringAngle = ring * ringStep * Mathf.Deg2Rad;
            Vector3 ringCenter = new(MathF.Cos(ringAngle) * ringRadius, 0, MathF.Sin(ringAngle) * ringRadius);

            for (int tube = 0; tube < res; ++tube)
            {
                float tubeAngle = tube * tubeStep * Mathf.Deg2Rad;
                verts[ring * res + tube] = ringCenter + new Vector3(MathF.Cos(tubeAngle) * tubeRadius, MathF.Sin(tubeAngle) * tubeRadius, 0);
            }
        }

        for (int ring = 0, ti = 0; ring < res; ++ring)
            for (int tube = 0; tube < res; tube++, ti += 6)
                AddQuad(tris, ti, ring * res + tube, ring * res + (tube + 1) % res, (ring + 1) % res * res + tube, (ring + 1) % res * res + (tube + 1) % res);

        return SetupMesh(new(), verts, tris);
    }

    static Mesh CreateGeodesicDome(float radius, int res)
    {
        var verts = new Vector3[(res + 1) * (res + 1)];
        var tris = new int[res * res * 6];

        for (int lat = 0; lat <= res; ++lat)
        {
            float theta = Mathf.PI * lat / res;
            for (int lon = 0; lon <= res; ++lon)
            {
                float phi = 2 * Mathf.PI * lon / res;
                verts[lat * (res + 1) + lon] = new(
                    radius * MathF.Sin(theta) * MathF.Cos(phi),
                    radius * MathF.Cos(theta),
                    radius * MathF.Sin(theta) * MathF.Sin(phi)
                );
            }
        }

        for (int lat = 0, ti = 0; lat < res; ++lat)
            for (int lon = 0; lon < res; ++lon, ti += 6)
                AddQuad(tris, ti, lat * (res + 1) + lon, lat * (res + 1) + lon + 1, (lat + 1) * (res + 1) + lon, (lat + 1) * (res + 1) + lon + 1);

        return SetupMesh(new(), verts, tris);
    }

    static void AddQuad(int[] tris, int i, int v0, int v1, int v2, int v3) =>
        (tris[i], tris[i + 1], tris[i + 2], tris[i + 3], tris[i + 4], tris[i + 5]) = (v0, v1, v2, v2, v1, v3);

    static void AddTriangle(int[] tris, int i, int v0, int v1, int v2) => (tris[i], tris[i + 1], tris[i + 2]) = (v0, v1, v2);

    static Mesh SetupMesh(Mesh mesh, Vector3[] verts, int[] tris)
    {
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }
}