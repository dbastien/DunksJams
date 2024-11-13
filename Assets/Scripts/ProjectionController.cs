using UnityEngine;

[ExecuteInEditMode]
public class ProjectionController : MonoBehaviour
{
    public enum ProjectionType 
    { 
        Orthographic, Cavalier, Cabinet, CustomOblique, 
        Isometric, Dimetric, Trimetric, Planometric,
        Perspective, TwoPointPerspective, ThreePointPerspective
    }

    public ProjectionType projectionType = ProjectionType.Orthographic;
    [Range(0, 90)] public float angle = 45f;               // Oblique angle for cavalier/cabinet
    [Range(0, 1)] public float cabinetRatio = 0.5f;        // Depth scaling for Cabinet projection
    public Vector2 customObliqueShear = new (1, 1);        // Custom shearing values for custom oblique
    [Range(0, 90)] public float dimetricAngleX = 42f;
    [Range(0, 90)] public float dimetricAngleY = 7f;

    Camera _camera;
    ProjectionType _currentProjection;
    
    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _currentProjection = projectionType;
        ApplyProjection();
    }
    
    void Update()
    {
        if (!_camera || projectionType == _currentProjection) return;
        _currentProjection = projectionType;
        ApplyProjection();
    }

    void ApplyProjection()
    {
        switch (projectionType)
        {
            case ProjectionType.Orthographic: _camera.SetOrthographic(); break;
            case ProjectionType.Cavalier: _camera.SetOblique(angle); break;
            case ProjectionType.Cabinet: _camera.SetOblique(angle, cabinetRatio); break;
            case ProjectionType.CustomOblique: _camera.SetOblique(customObliqueShear.x, customObliqueShear.y); break;
            case ProjectionType.Isometric: _camera.SetAxonometricProjection(30f, 45f); break;
            case ProjectionType.Dimetric: _camera.SetAxonometricProjection(dimetricAngleX, dimetricAngleY); break;
            case ProjectionType.Trimetric: _camera.SetAxonometricProjection(23f, 37f, 15f); break;
            case ProjectionType.Planometric: _camera.SetAxonometricProjection(45f, 45f); break;
            case ProjectionType.Perspective: _camera.SetPerspective(); break;
            case ProjectionType.TwoPointPerspective: _camera.SetPerspective(0, 30f); break;
            case ProjectionType.ThreePointPerspective: _camera.SetPerspective(15f, 30f, 10f); break;
        }
    }
}