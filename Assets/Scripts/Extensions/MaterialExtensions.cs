using UnityEngine;

public static class MaterialExtensions
{
    public static bool HasTexture(this Material mat, string prop) =>
        mat.HasProperty(prop) && mat.GetTexture(prop) != null;

    public static bool GetToggleOrDefault(this Material mat, string prop, bool defaultVal = false) =>
        mat.HasProperty(prop) ? mat.GetFloat(prop) == 1f : defaultVal;

    public static Color GetColorOrDefault(this Material mat, string prop, Color defaultVal = default) =>
        mat.HasProperty(prop) ? mat.GetColor(prop) : defaultVal;
    
    public static float GetFloatOrDefault(this Material mat, string prop, float defaultVal = 0f) =>
        mat.HasProperty(prop) ? mat.GetFloat(prop) : defaultVal;
    
    public static bool TryGetToggle(this Material mat, string prop, out bool value)
    {
        if (mat.HasProperty(prop))
        {
            value = mat.GetFloat(prop) == 1f;
            return true;
        }
        value = default;
        return false;
    }
    
    public static bool TryGetColor(this Material mat, string prop, out Color color)
    {
        if (mat.HasProperty(prop))
        {
            color = mat.GetColor(prop);
            return true;
        }
        color = default;
        return false;
    }
    
    public static bool TryGetFloat(this Material mat, string prop, out float value)
    {
        if (mat.HasProperty(prop))
        {
            value = mat.GetFloat(prop);
            return true;
        }
        value = default;
        return false;
    }
    
    public static void SetToggle(this Material mat, string prop, bool state) =>
        mat.SetFloat(prop, state ? 1f : 0f);
    
    public static void SetColorIfExists(this Material mat, string prop, Color color)
    {
        if (mat.HasProperty(prop)) mat.SetColor(prop, color);
    }
    
    public static void SetFloatIfExists(this Material mat, string prop, float value)
    {
        if (mat.HasProperty(prop)) mat.SetFloat(prop, value);
    }

    public static void SetKeyword(this Material mat, string keyword, bool state)
    {
        if (state) mat.EnableKeyword(keyword);
        else mat.DisableKeyword(keyword);
    }
}