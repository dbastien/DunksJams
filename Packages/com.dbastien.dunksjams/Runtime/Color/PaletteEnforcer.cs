using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enforces a palette on UI Graphics (Images, Text) and SpriteRenderers in a scope.
/// Useful for applying theme colors to a scene or prefab.
/// </summary>
public class PaletteEnforcer : MonoBehaviour
{
    public ColorPalette palette;
    [Tooltip("If enabled, applies palette to all children recursively.")]
    public bool applyToChildren = true;
    [Tooltip("Indices to apply: e.g., '0' for first color, '0,1,2' for first three.")]
    public string colorIndices = "0";

    public void ApplyPalette()
    {
        if (palette == null)
        {
            Debug.LogWarning("No palette assigned to PaletteEnforcer.", gameObject);
            return;
        }

        var indices = ParseIndices(colorIndices);
        ApplyToGraphics(indices);
        ApplyToSpriteRenderers(indices);
    }

    void ApplyToGraphics(int[] indices)
    {
        var graphics = applyToChildren
            ? GetComponentsInChildren<Graphic>()
            : GetComponents<Graphic>();

        for (int i = 0; i < graphics.Length; i++)
        {
            var idx = indices[i % indices.Length];
            graphics[i].color = palette.GetColor(idx, Color.white);
        }
    }

    void ApplyToSpriteRenderers(int[] indices)
    {
        var renderers = applyToChildren
            ? GetComponentsInChildren<SpriteRenderer>()
            : GetComponents<SpriteRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            var idx = indices[i % indices.Length];
            renderers[i].color = palette.GetColor(idx, Color.white);
        }
    }

    int[] ParseIndices(string str)
    {
        if (string.IsNullOrEmpty(str)) return new[] { 0 };

        var parts = str.Split(',');
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i].Trim(), out var idx))
                result[i] = Mathf.Max(0, idx);
        }
        return result;
    }
}