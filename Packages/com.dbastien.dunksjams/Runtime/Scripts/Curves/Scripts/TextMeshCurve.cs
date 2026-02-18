using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextMeshCurve : MonoBehaviour
{
    [NormalizedAnimationCurve] public AnimationCurve vertexCurve = new(new Keyframe(0, 0), new Keyframe(0.25f, 1.0f),
        new Keyframe(0.5f, 0), new Keyframe(0.75f, 1.0f), new Keyframe(1, 0f));

    [FloatIncremental(.1f)] public float curveScale = 1.0f;
    [FloatIncremental(.1f)] public float animationSpeed = 1.0f;

    public bool rotateLetters;

    [NormalizedAnimationCurve] public AnimationCurve rotationCurve = new(new Keyframe(0, 0), new Keyframe(0.25f, 1.0f),
        new Keyframe(0.5f, 0), new Keyframe(0.75f, 1.0f), new Keyframe(1, 0f));

    [Range(0.001f, .25f)] public float rotationScale = 0.05f;

    private float _timeElapsed;

    private TMP_Text _textComponent;
    private TMP_MeshInfo[] _initialMeshInfo;

    public void Update()
    {
        if (!_textComponent)
        {
            _textComponent = gameObject.GetComponent<TMP_Text>();
            _textComponent.havePropertiesChanged = true;
            _textComponent.ForceMeshUpdate();
            _initialMeshInfo = _textComponent.textInfo.CopyMeshInfoVertexData();
        }

        _timeElapsed += Time.deltaTime;

        _textComponent.havePropertiesChanged = true;
        _textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = _textComponent.textInfo;
        int characterCount = textInfo.characterCount;

        float boundsMinX = _textComponent.bounds.min.x;
        float boundsMaxX = _textComponent.bounds.max.x;
        float boundsDelta = boundsMaxX - boundsMinX;

        float animScaledTime = _timeElapsed * animationSpeed;

        for (var i = 0; i < characterCount; ++i)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            Vector3[] targetVertices = textInfo.meshInfo[materialIndex].vertices;
            Vector3[] initialVertices = _initialMeshInfo[materialIndex].vertices;

            // character baseline mid point
            var offsetToMidBaseline = new Vector3(
                (initialVertices[vertexIndex].x + initialVertices[vertexIndex + 2].x) * .5f,
                textInfo.characterInfo[i].baseLine,
                0f);

            // character position along curve
            float x0 = (offsetToMidBaseline.x - boundsMinX) /
                       boundsDelta; // Character's position relative to the bounds of the mesh.
            float y0 = vertexCurve.Evaluate(x0 + animScaledTime) * curveScale;

            Matrix4x4 matrix;
            if (rotateLetters)
            {
                // animScaledTime so synced with the translation motion
                float angle = (-.5f + rotationCurve.Evaluate(x0 + animScaledTime)) * rotationScale * 360f;

                Quaternion q = Quaternion.Euler(0, 0, angle);

                matrix = Matrix4x4.Translate(offsetToMidBaseline) *
                         Matrix4x4.TRS(new Vector3(0, y0, 0), q, Vector3.one) *
                         Matrix4x4.Translate(-offsetToMidBaseline);
            }
            else { matrix = Matrix4x4.Translate(new Vector3(0, y0, 0)); }

            targetVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(initialVertices[vertexIndex + 0]);
            targetVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(initialVertices[vertexIndex + 1]);
            targetVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(initialVertices[vertexIndex + 2]);
            targetVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(initialVertices[vertexIndex + 3]);
        }

        _textComponent.UpdateVertexData();
    }
}