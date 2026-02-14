using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Exports Unity meshes to Wavefront .obj format, with optional .mtl material files.
/// Works with raw Mesh objects or MeshFilter components.
/// </summary>
public static class ObjExporter
{
    /// <summary>Convert a Mesh to an OBJ-format string.</summary>
    public static string MeshToString(Mesh mesh, string objectName = "Exported")
    {
        var sb = new StringBuilder(mesh.vertexCount * 64);
        sb.AppendLine("# Exported from DunksJams");
        sb.Append("o ").AppendLine(objectName);

        foreach (var v in mesh.vertices)
            sb.Append("v ").Append(v.x).Append(' ').Append(v.y).Append(' ').AppendLine(v.z.ToString());

        if (mesh.normals.Length > 0)
            foreach (var n in mesh.normals)
                sb.Append("vn ").Append(n.x).Append(' ').Append(n.y).Append(' ').AppendLine(n.z.ToString());

        if (mesh.uv.Length > 0)
            foreach (var u in mesh.uv)
                sb.Append("vt ").Append(u.x).Append(' ').AppendLine(u.y.ToString());

        bool hasNormals = mesh.normals.Length > 0;
        bool hasUVs = mesh.uv.Length > 0;

        for (int sub = 0; sub < mesh.subMeshCount; ++sub)
        {
            if (mesh.subMeshCount > 1)
                sb.Append("g submesh_").AppendLine(sub.ToString());

            var tris = mesh.GetTriangles(sub);
            for (int i = 0; i < tris.Length; i += 3)
            {
                sb.Append("f ");
                for (int j = 0; j < 3; ++j)
                {
                    int idx = tris[i + j] + 1; // OBJ is 1-indexed
                    sb.Append(idx);
                    if (hasUVs || hasNormals)
                    {
                        sb.Append('/');
                        if (hasUVs) sb.Append(idx);
                        if (hasNormals) sb.Append('/').Append(idx);
                    }
                    if (j < 2) sb.Append(' ');
                }
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    /// <summary>Convert a MeshFilter to an OBJ-format string.</summary>
    public static string MeshToString(MeshFilter mf) =>
        MeshToString(mf.sharedMesh, mf.gameObject.name);

    /// <summary>Write a Mesh to an .obj file on disk.</summary>
    public static void MeshToFile(Mesh mesh, string path, string objectName = "Exported")
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, MeshToString(mesh, objectName));
    }

    /// <summary>Write a MeshFilter's mesh to an .obj file, optionally with a .mtl sidecar.</summary>
    public static void MeshToFile(MeshFilter mf, string path, bool exportMaterial = false)
    {
        MeshToFile(mf.sharedMesh, path, mf.gameObject.name);

        if (!exportMaterial) return;

        var renderer = mf.GetComponent<Renderer>();
        if (renderer == null || renderer.sharedMaterial == null) return;

        var mat = renderer.sharedMaterial;
        var mtlPath = Path.ChangeExtension(path, ".mtl");
        File.WriteAllText(mtlPath, MaterialToString(mat));
    }

    /// <summary>Generate a basic .mtl string for a material.</summary>
    public static string MaterialToString(Material mat)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Exported from DunksJams");
        sb.Append("newmtl ").AppendLine(mat.name);

        if (mat.HasColor("_Color"))
        {
            var c = mat.color;
            sb.Append("Ka ").Append(c.r * 0.2f).Append(' ').Append(c.g * 0.2f).Append(' ').AppendLine((c.b * 0.2f).ToString());
            sb.Append("Kd ").Append(c.r).Append(' ').Append(c.g).Append(' ').AppendLine(c.b.ToString());
        }
        else
        {
            sb.AppendLine("Ka 0.2 0.2 0.2");
            sb.AppendLine("Kd 0.8 0.8 0.8");
        }

        sb.AppendLine("Ks 0.9 0.9 0.9");
        sb.AppendLine("Ns 10.0");
        sb.AppendLine("d 1.0");
        sb.AppendLine("illum 2");

        if (mat.mainTexture != null)
            sb.Append("map_Kd ").AppendLine(mat.mainTexture.name);

        return sb.ToString();
    }
}
