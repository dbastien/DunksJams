using UnityEngine;

[CreateAssetMenu(fileName = "PaletteSettings", menuName = "DunksJams/Palette Settings")]
public class PaletteSettings : ScriptableObject
{
    [Tooltip(
        "Global default palette. Place this asset under a Resources folder and name it 'PaletteSettings' to be loaded automatically at runtime.")]
    public ColorPalette defaultPalette;

    /// <summary>
    /// Load the PaletteSettings asset from Resources (expects Resources/PaletteSettings.asset).
    /// Returns null if not found.
    /// </summary>
    public static PaletteSettings Load() => Resources.Load<PaletteSettings>("PaletteSettings");
}