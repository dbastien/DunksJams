using UnityEngine;
using TMPro;

public class TMP_AnimatedText : MonoBehaviour
{
    public float waveSpeed = 5f;
    public float waveHeight = 2f;

    private TMP_Text _textComponent;
    private bool _hasTextChanged;

    private void Awake() => _textComponent = GetComponent<TMP_Text>();

    private void OnEnable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

    private void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

    private void OnTextChanged(Object obj)
    {
        if (obj == _textComponent) _hasTextChanged = true;
    }

    private void LateUpdate()
    {
        // Only animate if we have text and it's visible
        if (_textComponent == null || _textComponent.textInfo.characterCount == 0) return;

        _textComponent.ForceMeshUpdate();
        var textInfo = _textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            // Simple squiggle/wave effect
            // We could restrict this to specific characters by checking tags,
            // but for now let's just show the capability.

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;

            float offset = Mathf.Sin(Time.time * waveSpeed + i * 0.5f) * waveHeight;

            sourceVertices[vertexIndex + 0].y += offset;
            sourceVertices[vertexIndex + 1].y += offset;
            sourceVertices[vertexIndex + 2].y += offset;
            sourceVertices[vertexIndex + 3].y += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}