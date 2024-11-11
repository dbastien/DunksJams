using UnityEngine;

public static class Color32Extensions
{
    public static Color32 PremultiplyAlpha(this Color32 c)
    {
        Color floatCol = c;        
        return (Color32)floatCol.PremultiplyAlpha(); 
    }
}