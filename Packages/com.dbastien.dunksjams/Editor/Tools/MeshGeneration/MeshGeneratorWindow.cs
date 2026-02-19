using UnityEditor;
using UnityEngine;

public class MeshGeneratorWindow : EditorWindow
{
    private enum ShapeType
    {
        Box,
        Sphere,
        GeoSphere,
        Cylinder,
        Cone,
        Capsule,
        Prism,
        Pyramid,
        Grid,
        Ring,
        Torus,
        TorusKnot,
        Helix,
        Tube,
        Arc,
        SuperEllipsoid,
        Ellipsoid,
        Disc,
        Triangle
    }

    private ShapeType _shape = ShapeType.Box;
    private NormalsType _normals = NormalsType.Vertex;
    private PivotPosition _pivot = PivotPosition.Center;
    private Vector2 _scroll;

    // Shared
    private float _width = 1f, _height = 1f, _depth = 1f;
    private float _radius = 0.5f, _radius2;
    private int _segments = 16;
    private int _widthSegs = 1, _heightSegs = 1, _depthSegs = 1;
    private int _resX = 10, _resZ = 10;

    // Torus
    private float _torusRadius = 0.5f, _tubeRadius = 0.2f;
    private int _torusSegs = 24, _tubeSegs = 12;

    // Torus Knot
    private int _knotP = 2, _knotQ = 3;

    // Helix
    private float _helixRadius = 0.5f, _helixHeight = 2f, _helixTubeRadius = 0.1f;
    private int _helixTurns = 3, _helixSegs = 64, _helixTubeSegs = 8;

    // Prism
    private int _prismSides = 6;

    // Ring
    private float _innerRadius = 0.3f, _outerRadius = 0.5f;

    // Tube
    private float _tubeInner = 0.3f, _tubeOuter = 0.5f, _tubeHeight = 1f;
    private int _tubeSides = 16, _tubeHeightSegs = 1;

    // SuperEllipsoid
    private float _seN1 = 1f, _seN2 = 1f;
    private float _seWidth = 0.5f, _seHeight = 0.5f, _seLength = 0.5f;
    private int _seSegments = 16;

    // Arc
    private float _arcWidth = 1f, _arcHeight = 1f, _arcDepth = 1f;
    private int _arcSegs = 8;

    // GeoSphere
    private int _geoSubdiv = 2;
    private GeoSphereGenerator.BaseType _geoBase = GeoSphereGenerator.BaseType.Icosahedron;

    // Ellipsoid
    private float _ellWidth = 0.5f, _ellHeight = 0.5f, _ellDepth = 0.5f;
    private int _ellSegments = 16;

    // Disc
    private float _discRadiusX = 0.5f, _discRadiusZ = 0.5f;
    private int _discSegments = 32;

    // Triangle
    private float _triSize = 1f;

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        _shape = (ShapeType)EditorGUILayout.EnumPopup("Shape", _shape);
        EditorGUILayout.Space(4);

        DrawShapeParams();

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Create", GUILayout.Height(30)))
            CreateShape();

        EditorGUILayout.EndScrollView();
    }

    private void DrawShapeParams()
    {
        switch (_shape)
        {
            case ShapeType.Box:
                _width = EditorGUILayout.FloatField("Width", _width);
                _height = EditorGUILayout.FloatField("Height", _height);
                _depth = EditorGUILayout.FloatField("Depth", _depth);
                _widthSegs = EditorGUILayout.IntSlider("Width Segments", _widthSegs, 1, 50);
                _heightSegs = EditorGUILayout.IntSlider("Height Segments", _heightSegs, 1, 50);
                _depthSegs = EditorGUILayout.IntSlider("Depth Segments", _depthSegs, 1, 50);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Sphere:
                _radius = EditorGUILayout.FloatField("Radius", _radius);
                _segments = EditorGUILayout.IntSlider("Segments", _segments, 4, 64);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.GeoSphere:
                _radius = EditorGUILayout.FloatField("Radius", _radius);
                _geoSubdiv = EditorGUILayout.IntSlider("Subdivision", _geoSubdiv, 0, 5);
                _geoBase = (GeoSphereGenerator.BaseType)EditorGUILayout.EnumPopup("Base", _geoBase);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Cylinder:
                _radius = EditorGUILayout.FloatField("Radius", _radius);
                _height = EditorGUILayout.FloatField("Height", _height);
                _segments = EditorGUILayout.IntSlider("Sides", _segments, 3, 64);
                _heightSegs = EditorGUILayout.IntSlider("Height Segments", _heightSegs, 1, 50);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Cone:
                _radius = EditorGUILayout.FloatField("Bottom Radius", _radius);
                _radius2 = EditorGUILayout.FloatField("Top Radius", _radius2);
                _height = EditorGUILayout.FloatField("Height", _height);
                _segments = EditorGUILayout.IntSlider("Sides", _segments, 3, 64);
                _heightSegs = EditorGUILayout.IntSlider("Height Segments", _heightSegs, 1, 50);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Capsule:
                _radius = EditorGUILayout.FloatField("Radius", _radius);
                _height = EditorGUILayout.FloatField("Height", _height);
                _segments = EditorGUILayout.IntSlider("Segments", _segments, 4, 64);
                _heightSegs = EditorGUILayout.IntSlider("Height Segments", _heightSegs, 1, 50);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Prism:
                _prismSides = EditorGUILayout.IntSlider("Sides", _prismSides, 3, 32);
                _radius = EditorGUILayout.FloatField("Radius", _radius);
                _height = EditorGUILayout.FloatField("Height", _height);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Pyramid:
                _width = EditorGUILayout.FloatField("Base Size", _width);
                _height = EditorGUILayout.FloatField("Height", _height);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Grid:
                _width = EditorGUILayout.FloatField("Width", _width);
                _depth = EditorGUILayout.FloatField("Depth", _depth);
                _resX = EditorGUILayout.IntSlider("Resolution X", _resX, 1, 100);
                _resZ = EditorGUILayout.IntSlider("Resolution Z", _resZ, 1, 100);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Ring:
                _innerRadius = EditorGUILayout.FloatField("Inner Radius", _innerRadius);
                _outerRadius = EditorGUILayout.FloatField("Outer Radius", _outerRadius);
                _segments = EditorGUILayout.IntSlider("Segments", _segments, 3, 64);
                break;

            case ShapeType.Torus:
                _torusRadius = EditorGUILayout.FloatField("Torus Radius", _torusRadius);
                _tubeRadius = EditorGUILayout.FloatField("Tube Radius", _tubeRadius);
                _torusSegs = EditorGUILayout.IntSlider("Torus Segments", _torusSegs, 3, 128);
                _tubeSegs = EditorGUILayout.IntSlider("Tube Segments", _tubeSegs, 3, 64);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.TorusKnot:
                _torusRadius = EditorGUILayout.FloatField("Torus Radius", _torusRadius);
                _tubeRadius = EditorGUILayout.FloatField("Tube Radius", _tubeRadius);
                _torusSegs = EditorGUILayout.IntSlider("Torus Segments", _torusSegs, 3, 128);
                _tubeSegs = EditorGUILayout.IntSlider("Tube Segments", _tubeSegs, 3, 32);
                _knotP = EditorGUILayout.IntSlider("P", _knotP, 1, 10);
                _knotQ = EditorGUILayout.IntSlider("Q", _knotQ, 1, 10);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Helix:
                _helixRadius = EditorGUILayout.FloatField("Radius", _helixRadius);
                _helixHeight = EditorGUILayout.FloatField("Height", _helixHeight);
                _helixTubeRadius = EditorGUILayout.FloatField("Tube Radius", _helixTubeRadius);
                _helixTurns = EditorGUILayout.IntSlider("Turns", _helixTurns, 1, 20);
                _helixSegs = EditorGUILayout.IntSlider("Segments", _helixSegs, 8, 256);
                _helixTubeSegs = EditorGUILayout.IntSlider("Tube Segments", _helixTubeSegs, 3, 32);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Tube:
                _tubeInner = EditorGUILayout.FloatField("Inner Radius", _tubeInner);
                _tubeOuter = EditorGUILayout.FloatField("Outer Radius", _tubeOuter);
                _tubeHeight = EditorGUILayout.FloatField("Height", _tubeHeight);
                _tubeSides = EditorGUILayout.IntSlider("Sides", _tubeSides, 3, 64);
                _tubeHeightSegs = EditorGUILayout.IntSlider("Height Segments", _tubeHeightSegs, 1, 50);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Arc:
                _arcWidth = EditorGUILayout.FloatField("Width", _arcWidth);
                _arcHeight = EditorGUILayout.FloatField("Height", _arcHeight);
                _arcDepth = EditorGUILayout.FloatField("Depth", _arcDepth);
                _arcSegs = EditorGUILayout.IntSlider("Arc Segments", _arcSegs, 1, 32);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.SuperEllipsoid:
                _seWidth = EditorGUILayout.FloatField("Width", _seWidth);
                _seHeight = EditorGUILayout.FloatField("Height", _seHeight);
                _seLength = EditorGUILayout.FloatField("Length", _seLength);
                _seSegments = EditorGUILayout.IntSlider("Segments", _seSegments, 1, 32);
                _seN1 = EditorGUILayout.Slider("N1", _seN1, 0.01f, 5f);
                _seN2 = EditorGUILayout.Slider("N2", _seN2, 0.01f, 5f);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Ellipsoid:
                _ellWidth = EditorGUILayout.FloatField("Width", _ellWidth);
                _ellHeight = EditorGUILayout.FloatField("Height", _ellHeight);
                _ellDepth = EditorGUILayout.FloatField("Depth", _ellDepth);
                _ellSegments = EditorGUILayout.IntSlider("Segments", _ellSegments, 4, 64);
                _normals = (NormalsType)EditorGUILayout.EnumPopup("Normals", _normals);
                _pivot = (PivotPosition)EditorGUILayout.EnumPopup("Pivot", _pivot);
                break;

            case ShapeType.Disc:
                _discRadiusX = EditorGUILayout.FloatField("Radius X", _discRadiusX);
                _discRadiusZ = EditorGUILayout.FloatField("Radius Z", _discRadiusZ);
                _discSegments = EditorGUILayout.IntSlider("Segments", _discSegments, 3, 128);
                break;

            case ShapeType.Triangle:
                _triSize = EditorGUILayout.FloatField("Size", _triSize);
                break;
        }
    }

    private void CreateShape()
    {
        Mesh mesh = _shape switch
        {
            ShapeType.Box => BoxGenerator.Generate(_width, _height, _depth, _widthSegs, _heightSegs, _depthSegs,
                _pivot),
            ShapeType.Sphere => SphereGenerator.Generate(_radius, _segments, _normals, _pivot),
            ShapeType.GeoSphere => GeoSphereGenerator.Generate(_radius, _geoSubdiv, _geoBase, _normals, _pivot),
            ShapeType.Cylinder =>
                CylinderGenerator.Generate(_radius, _height, _segments, _heightSegs, _normals, _pivot),
            ShapeType.Cone => ConeGenerator.Generate(_radius, _radius2, _height, _segments, _heightSegs, _normals,
                _pivot),
            ShapeType.Capsule => CapsuleGenerator.Generate(_radius, _height, _segments, _heightSegs, _normals, _pivot),
            ShapeType.Prism => PrismGenerator.Generate(_prismSides, _radius, _height, _pivot),
            ShapeType.Pyramid => PyramidGenerator.Generate(_width, _height, _pivot),
            ShapeType.Grid => GridGenerator.Generate(_width, _depth, _resX, _resZ, _pivot),
            ShapeType.Ring => RingGenerator.Generate(_innerRadius, _outerRadius, _segments),
            ShapeType.Torus => TorusGenerator.Generate(_torusRadius, _tubeRadius, _torusSegs, _tubeSegs, _normals,
                _pivot),
            ShapeType.TorusKnot => TorusKnotGenerator.Generate(_torusRadius, _tubeRadius, _torusSegs, _tubeSegs, _knotP,
                _knotQ, _normals, _pivot),
            ShapeType.Helix => HelixGenerator.Generate(_helixRadius, _helixHeight, _helixTubeRadius, _helixTurns,
                _helixSegs, _helixTubeSegs, _pivot),
            ShapeType.Tube => TubeGenerator.Generate(_tubeInner, _tubeOuter, _tubeHeight, _tubeSides, _tubeHeightSegs,
                _normals, _pivot),
            ShapeType.Arc => ArcGenerator.Generate(_arcWidth, _arcHeight, _arcDepth, _arcSegs, null, _pivot),
            ShapeType.SuperEllipsoid => SuperEllipsoidGenerator.Generate(_seWidth, _seHeight, _seLength, _seSegments,
                _seN1, _seN2, _normals, _pivot),
            ShapeType.Ellipsoid => EllipsoidGenerator.Generate(_ellWidth, _ellHeight, _ellDepth, _ellSegments, _normals,
                _pivot),
            ShapeType.Disc => DiscGenerator.Generate(_discRadiusX, _discRadiusZ, _discSegments),
            ShapeType.Triangle => TriangleGenerator.Generate(_triSize),
            _ => null
        };

        if (mesh == null) return;

        var go = new GameObject(_shape.ToString());
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = DefaultResources.Material;
        mf.sharedMesh = mesh;
        Undo.RegisterCreatedObjectUndo(go, "Create " + _shape);
        Selection.activeObject = go;
    }
}