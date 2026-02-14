using UnityEditor;
using UnityEngine;

public static class AdvancedMeshMenu
{
    // -- Basic shapes --
    [MenuItem("‽/GameObject/3D Object/Box")] public static void CreateBox() => Create("Box", BoxGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Sphere")] public static void CreateSphere() => Create("Sphere", SphereGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/GeoSphere")] public static void CreateGeoSphere() => Create("GeoSphere", GeoSphereGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Cylinder")] public static void CreateCylinder() => Create("Cylinder", CylinderGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Cone")] public static void CreateCone() => Create("Cone", ConeGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Capsule")] public static void CreateCapsule() => Create("Capsule", CapsuleGenerator.Generate());

    // -- Prisms --
    [MenuItem("‽/GameObject/3D Object/Triangular Prism")] public static void CreateTriPrism() => Create("TriangularPrism", PrismGenerator.Generate(3));
    [MenuItem("‽/GameObject/3D Object/Hexagonal Prism")] public static void CreateHexPrism() => Create("HexagonalPrism", PrismGenerator.Generate(6));
    [MenuItem("‽/GameObject/3D Object/Pyramid")] public static void CreatePyramid() => Create("Pyramid", PyramidGenerator.Generate());

    // -- Surfaces --
    [MenuItem("‽/GameObject/3D Object/Grid")] public static void CreateGrid() => Create("Grid", GridGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Ring")] public static void CreateRing() => Create("Ring", RingGenerator.Generate());

    // -- Curved --
    [MenuItem("‽/GameObject/3D Object/Torus")] public static void CreateTorus() => Create("Torus", TorusGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Torus Knot")] public static void CreateTorusKnot() => Create("TorusKnot", TorusKnotGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Helix")] public static void CreateHelix() => Create("Helix", HelixGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Tube")] public static void CreateTube() => Create("Tube", TubeGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Arc")] public static void CreateArc() => Create("Arc", ArcGenerator.Generate());

    // -- Flat --
    [MenuItem("‽/GameObject/3D Object/Disc")] public static void CreateDisc() => Create("Disc", DiscGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Triangle")] public static void CreateTriangle() => Create("Triangle", TriangleGenerator.Generate());

    // -- Parametric --
    [MenuItem("‽/GameObject/3D Object/Ellipsoid")] public static void CreateEllipsoid() => Create("Ellipsoid", EllipsoidGenerator.Generate());
    [MenuItem("‽/GameObject/3D Object/Super Ellipsoid")] public static void CreateSuperEllipsoid() => Create("SuperEllipsoid", SuperEllipsoidGenerator.Generate());

    // -- Tools --
    [MenuItem("‽/Tools/Mesh Generator")]
    public static void OpenMeshGeneratorWindow() => EditorWindow.GetWindow<MeshGeneratorWindow>("Mesh Generator");

    [MenuItem("‽/Tools/Mesh Cutter")]
    public static void OpenMeshCutterWindow() => EditorWindow.GetWindow<MeshCutterWindow>("Mesh Cutter");

    [MenuItem("‽/Tools/Export Mesh to OBJ")]
    public static void ExportSelectedMeshToObj() => MeshExportMenu.ExportSelectedToObj();

    static void Create(string name, Mesh mesh)
    {
        var go = new GameObject(name);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = DefaultResources.Material;
        mf.sharedMesh = mesh;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        Selection.activeObject = go;
    }
}
