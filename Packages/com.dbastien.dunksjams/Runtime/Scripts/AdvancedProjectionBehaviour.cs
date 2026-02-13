using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public sealed class AdvancedProjectionBehaviour : MonoBehaviour
{
    public enum ProjectionType
    {
        Orthographic,
        Cavalier,
        Cabinet,
        CustomOblique,
        Isometric,
        Dimetric,
        Trimetric,
        Planometric,
        Perspective,
        TwoPointPerspective,
        ThreePointPerspective
    }

    public ProjectionType projectionType = ProjectionType.Orthographic;

    [Range(0, 90)] public float angle = 45f;
    [Range(0, 1)] public float cabinetRatio = 0.5f;
    public Vector2 customObliqueShear = new(1f, 1f);

    [Range(0, 90)] public float dimetricAngleX = 42f;
    [Range(0, 90)] public float dimetricAngleY = 7f;

    [Tooltip("If true, re-applies every frame (for animated params).")]
    public bool dynamicUpdates;

    Camera _cam;

    // Cache last applied state to avoid redundant work
    ProjectionType _lastType;
    float _lastAngle, _lastCabinetRatio;
    Vector2 _lastShear;
    float _lastDimX, _lastDimY;

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        ApplyIfChanged(true);
    }

    void OnValidate()
    {
        // In editor, apply immediately when values change.
        if (!isActiveAndEnabled) return;
        if (_cam == null) _cam = GetComponent<Camera>();
        ApplyIfChanged(true);
    }

    void Update()
    {
        if (!dynamicUpdates) return;
        ApplyIfChanged(false);
    }

    void ApplyIfChanged(bool force)
    {
        if (_cam == null) return;

        var changed =
            force ||
            _lastType != projectionType ||
            !Mathf.Approximately(_lastAngle, angle) ||
            !Mathf.Approximately(_lastCabinetRatio, cabinetRatio) ||
            _lastShear != customObliqueShear ||
            !Mathf.Approximately(_lastDimX, dimetricAngleX) ||
            !Mathf.Approximately(_lastDimY, dimetricAngleY);

        if (!changed) return;

        _lastType = projectionType;
        _lastAngle = angle;
        _lastCabinetRatio = cabinetRatio;
        _lastShear = customObliqueShear;
        _lastDimX = dimetricAngleX;
        _lastDimY = dimetricAngleY;

        ApplyProjection();
    }

    void ApplyProjection()
    {
        switch (projectionType)
        {
            case ProjectionType.Orthographic: _cam.SetOrthographic(); break;

            case ProjectionType.Cavalier:      _cam.SetOblique(angle); break;
            case ProjectionType.Cabinet:       _cam.SetOblique(angle, cabinetRatio); break;
            case ProjectionType.CustomOblique: _cam.SetOblique(customObliqueShear.x, customObliqueShear.y); break;

            case ProjectionType.Isometric:   _cam.SetAxonometricProjection(30f, 45f); break;
            case ProjectionType.Dimetric:    _cam.SetAxonometricProjection(dimetricAngleX, dimetricAngleY); break;
            case ProjectionType.Trimetric:   _cam.SetAxonometricProjection(23f, 37f, 15f); break;
            case ProjectionType.Planometric: _cam.SetAxonometricProjection(45f, 45f); break;

            case ProjectionType.Perspective:           _cam.SetPerspective(); break;
            case ProjectionType.TwoPointPerspective:   _cam.SetPerspective(0f, 30f); break;
            case ProjectionType.ThreePointPerspective: _cam.SetPerspective(15f, 30f, 10f); break;
        }
    }
}