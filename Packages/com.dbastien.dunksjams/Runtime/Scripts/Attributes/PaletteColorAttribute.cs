using System;
using UnityEngine;

/// <summary>
/// Apply this attribute to a Color field to show a small palette swatch picker next to the color field in the Inspector.
/// The optional <c>palettePath</c> can point to a specific ColorPalette asset (relative project path) to use.
/// If empty, the drawer will pick the first available ColorPalette asset.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PaletteColorAttribute : PropertyAttribute
{
    public readonly string palettePath;

    public PaletteColorAttribute(string palettePath = null) => this.palettePath = palettePath;
}