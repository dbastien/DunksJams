using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextEffects : MonoBehaviour
{
    public float typingSpeed = 0.05f, fadeDuration = 0.2f, referenceFontSize = 36f;
    public Vector3 bounceScale = new(1.2f, 1.2f, 1.2f);
    public AudioClip typingSound;
    public bool useDynamicScaling = true;

    private TextMeshProUGUI _tmp;
    private string _originalText;
    private AudioSource _audioSource;
    private Dictionary<int, (string tag, string value)> _taggedEvents = new();
    private List<int> _eventIndexes = new();

    private void Awake()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void DisplayText(string newText)
    {
        StopAllCoroutines();
        _originalText = ParseText(newText);
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        _tmp.text = "";
        _tmp.ForceMeshUpdate();
        for (var i = 0; i < _originalText.Length; ++i)
        {
            if (_originalText[i] == '<' && ProcessTag(ref i)) continue;

            _tmp.text += _originalText[i];
            _tmp.ForceMeshUpdate();

            if (!char.IsWhiteSpace(_originalText[i]))
            {
                StartBounceFade(i);
                PlayTypingSound();
                if (_eventIndexes.Contains(i)) TriggerCharacterEvent(i);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void StartBounceFade(int idx)
    {
        TMP_TextInfo textInfo = _tmp.textInfo;
        int matIdx = textInfo.characterInfo[idx].materialReferenceIndex;
        int vertIdx = textInfo.characterInfo[idx].vertexIndex;
        Vector3[] vertices = textInfo.meshInfo[matIdx].vertices;
        Vector3 origScale = vertices[vertIdx];

        StartCoroutine(BounceFadeCoroutine(idx, origScale));
    }

    private IEnumerator BounceFadeCoroutine(int idx, Vector3 origScale)
    {
        TMP_TextInfo textInfo = _tmp.textInfo;
        int matIdx = textInfo.characterInfo[idx].materialReferenceIndex;
        int vertIdx = textInfo.characterInfo[idx].vertexIndex;
        Vector3[] verts = textInfo.meshInfo[matIdx].vertices;

        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            float easeT = Ease.SmootherStep(t / fadeDuration);
            for (var j = 0; j < 4; ++j)
                verts[vertIdx + j] = Vector3.Lerp(origScale, origScale.Scaled(bounceScale), easeT);

            _tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            yield return null;
        }
    }

    private void PlayTypingSound()
    {
        if (typingSound) _audioSource.PlayOneShot(typingSound, Rand.Float() * 0.2f + 0.9f);
    }

    private void TriggerCharacterEvent(int idx)
    {
        if (_taggedEvents.TryGetValue(idx, out (string tag, string value) tagData))
            // Custom event handling here (could log or invoke additional behavior)
            DLog.Log($"Triggered event '{tagData.tag}' with value '{tagData.value}' at index {idx}");
    }

    private bool ProcessTag(ref int idx)
    {
        int closingTag = _originalText.IndexOf('>', idx);
        if (closingTag == -1) return false;

        string tagContent = _originalText.Substring(idx + 1, closingTag - idx - 1);
        string[] split = tagContent.Split('=');
        string tag = split[0];
        string val = split.Length > 1 ? split[1] : null;

        if (tag == "event") _eventIndexes.Add(_tmp.text.Length);
        _taggedEvents[_tmp.text.Length] = (tag, val);

        idx = closingTag;
        return true;
    }

    private string ParseText(string text)
    {
        var parsed = new StringBuilder();
        for (var i = 0; i < text.Length; ++i)
        {
            if (text[i] == '[')
            {
                int closeBracket = text.IndexOf(']', i);
                if (closeBracket > i)
                {
                    string tagContent = text.Substring(i + 1, closeBracket - i - 1);
                    if (ProcessBracketTag(parsed, tagContent))
                    {
                        i = closeBracket;
                        continue;
                    }
                }
            }

            parsed.Append(text[i]);
        }

        return parsed.ToString();
    }

    private bool ProcessBracketTag(StringBuilder parsed, string tagContent)
    {
        string[] split = tagContent.Split('=');
        string val = split.Length > 1 ? split[1] : null;
        string tag = split[0];
        switch (tag)
        {
            case "event":
                _eventIndexes.Add(parsed.Length);
                _taggedEvents[parsed.Length] = (tag, val);
                break;
            case "color":
                parsed.Append($"<color={val}>");
                break;
            case "/color":
                parsed.Append("</color>");
                break;
            case "size":
                parsed.Append($"<size={val}%>");
                break;
            case "/size":
                parsed.Append("</size>");
                break;
            default:
                return false;
        }

        return true;
    }
}