using UnityEngine;

public class ComponentMemberReferenceCurve : MonoBehaviour
{
    public enum CurveType
    {
        Float,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
        Color
    }

    [SerializeField] private GameObject selectedGameObject;
    [SerializeField] public ComponentMemberReference memberReference;

    [SerializeField] [NormalizedAnimationCurve]
    public AnimationCurve curve = new();

    [SerializeField] public CurveType curveType;
    [SerializeField] public float lengthScale = 1f;

    //[SerializeField] public bool relativeMode;
    //curveOffset - how to do, one for each type? :(

    [SerializeField] public float startFloat, endFloat;
    [SerializeField] public Vector2 startVector2, endVector2;
    [SerializeField] public Vector3 startVector3, endVector3;
    [SerializeField] public Vector4 startVector4, endVector4;
    [SerializeField] public Quaternion startQuaternion, endQuaternion;
    [SerializeField] public Color startColor, endColor;

    private System.Action<float> _applyValue;

    private void Start() => CacheApplyMethod();

    private void CacheApplyMethod()
    {
        if (!memberReference.IsValid())
        {
            DLog.LogE("MemberReference not valid.");
            enabled = false;
            return;
        }

        _applyValue = curveType switch
        {
            CurveType.Float => t => memberReference.SetValue(Mathf.LerpUnclamped(startFloat, endFloat, t)),
            CurveType.Vector2 => t => memberReference.SetValue(Vector2.LerpUnclamped(startVector2, endVector2, t)),
            CurveType.Vector3 => t => memberReference.SetValue(Vector3.LerpUnclamped(startVector3, endVector3, t)),
            CurveType.Vector4 => t => memberReference.SetValue(Vector4.LerpUnclamped(startVector4, endVector4, t)),
            CurveType.Quaternion => t =>
                memberReference.SetValue(Quaternion.LerpUnclamped(startQuaternion, endQuaternion, t)),
            CurveType.Color => t => memberReference.SetValue(Color.LerpUnclamped(startColor, endColor, t)),
            _ => null
        };
    }

    private void Awake()
    {
        // if (relativeMode) {
        //     curveStart = currentPos;
        //     curveEnd = currentPos + curveOffset;
        // }
    }

    private void Update()
    {
        if (_applyValue == null)
        {
            DLog.LogW("_applyValue == null");
            return;
        }

        float t = Mathf.Repeat(Time.time / lengthScale, 1f);
        float curveValue = curve.Evaluate(t);

        _applyValue(curveValue);
    }

    private void Reset() => ResetStartEnd();

    private void ResetStartEnd()
    {
        // if (!relativeMode) {} else {}
    }
}