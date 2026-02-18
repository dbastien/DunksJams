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

        int[] indices = ParseIndices(colorIndices);
        ApplyToGraphics(indices);
        ApplyToSpriteRenderers(indices);
    }

    private void ApplyToGraphics(int[] indices)
    {
        Graphic[] graphics = applyToChildren
            ? GetComponentsInChildren<Graphic>()
            : GetComponents<Graphic>();

        for (var i = 0; i < graphics.Length; i++)
        {
            int idx = indices[i % indices.Length];
            graphics[i].color = palette.GetColor(idx, Color.white);
        }
    }

    private void ApplyToSpriteRenderers(int[] indices)
    {
        SpriteRenderer[] renderers = applyToChildren
            ? GetComponentsInChildren<SpriteRenderer>()
            : GetComponents<SpriteRenderer>();

        for (var i = 0; i < renderers.Length; i++)
        {
            int idx = indices[i % indices.Length];
            renderers[i].color = palette.GetColor(idx, Color.white);
        }
    }

    private int[] ParseIndices(string str)
    {
        if (string.IsNullOrEmpty(str)) return new[] { 0 };

        string[] parts = str.Split(',');
        var result = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            if (int.TryParse(parts[i].Trim(), out int idx))
                result[i] = Mathf.Max(0, idx);
        return result;
    }
}