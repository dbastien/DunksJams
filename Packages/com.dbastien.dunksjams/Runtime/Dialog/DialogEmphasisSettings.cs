using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EmphasisStyle
{
    public string name = "em1";
    public Color color = Color.yellow;
    public bool bold = true;
    public bool italic;
    public bool squiggle;

    public string Apply(string text)
    {
        string result = text;
        if (bold) result = $"<b>{result}</b>";
        if (italic) result = $"<i>{result}</i>";
        if (squiggle) result = $"<voffset=0.1em>{result}</voffset>"; // Placeholder for squiggle detection
        string hex = ColorUtility.ToHtmlStringRGB(color);
        result = $"<color=#{hex}>{result}</color>";
        return result;
    }
}

[CreateAssetMenu(fileName = "EmphasisSettings", menuName = "Interroband/Dialog/Emphasis Settings")]
public class DialogEmphasisSettings : ScriptableObject
{
    public List<EmphasisStyle> styles = new();

    public string ProcessEmphasis(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string result = text;
        foreach (EmphasisStyle style in styles)
        {
            var tag = $"[{style.name}]";
            var endTag = $"[/{style.name}]";

            // This is a simple replacement, more robust regex could be used
            int index;
            while ((index = result.IndexOf(tag)) != -1)
            {
                int endIndex = result.IndexOf(endTag, index);
                if (endIndex != -1)
                {
                    string content = result.Substring(index + tag.Length, endIndex - index - tag.Length);
                    string replaced = style.Apply(content);
                    result = result.Remove(index, endIndex + endTag.Length - index).Insert(index, replaced);
                }
                else { break; }
            }
        }

        return result;
    }
}