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

    TextMeshProUGUI _tmp;
    string _originalText;
    AudioSource _audioSource;
    Dictionary<int, (string tag, string value)> _taggedEvents = new();
    List<int> _eventIndexes = new();

    void Awake()
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

    IEnumerator AnimateText()
    {
        _tmp.text = "";
        _tmp.ForceMeshUpdate();
        for (int i = 0; i < _originalText.Length; ++i)
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

    void StartBounceFade(int idx)
    {
        var textInfo = _tmp.textInfo;
        int matIdx = textInfo.characterInfo[idx].materialReferenceIndex;
        int vertIdx = textInfo.characterInfo[idx].vertexIndex;
        var vertices = textInfo.meshInfo[matIdx].vertices;
        Vector3 origScale = vertices[vertIdx];
        
        StartCoroutine(BounceFadeCoroutine(idx, origScale));
    }

    IEnumerator BounceFadeCoroutine(int idx, Vector3 origScale)
    {
        var textInfo = _tmp.textInfo;
        int matIdx = textInfo.characterInfo[idx].materialReferenceIndex;
        int vertIdx = textInfo.characterInfo[idx].vertexIndex;
        var verts = textInfo.meshInfo[matIdx].vertices;

        for (float t = 0; t <= fadeDuration; t += Time.deltaTime)
        {
            float easeT = Ease.SmootherStep(t / fadeDuration);
            for (int j = 0; j < 4; ++j)
                verts[vertIdx + j] = Vector3.Lerp(origScale, origScale.Scaled(bounceScale), easeT);

            _tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            yield return null;
        }
    }

    void PlayTypingSound()
    {
        if (typingSound) _audioSource.PlayOneShot(typingSound, Rand.Float() * 0.2f + 0.9f);
    }

    void TriggerCharacterEvent(int idx)
    {
        if (_taggedEvents.TryGetValue(idx, out var tagData))
        {
            // Custom event handling here (could log or invoke additional behavior)
            DLog.Log($"Triggered event '{tagData.tag}' with value '{tagData.value}' at index {idx}");
        }
    }

    bool ProcessTag(ref int idx)
    {
        int closingTag = _originalText.IndexOf('>', idx);
        if (closingTag == -1) return false;
        
        string tagContent = _originalText.Substring(idx + 1, closingTag - idx - 1);
        var split = tagContent.Split('=');
        string tag = split[0];
        string val = split.Length > 1 ? split[1] : null;
        
        if (tag == "event") _eventIndexes.Add(_tmp.text.Length);
        _taggedEvents[_tmp.text.Length] = (tag, val);

        idx = closingTag;
        return true;
    }

    string ParseText(string text)
    {
        var parsed = new StringBuilder();
        for (int i = 0; i < text.Length; ++i)
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

    bool ProcessBracketTag(StringBuilder parsed, string tagContent)
    {
        var split = tagContent.Split('=');
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
