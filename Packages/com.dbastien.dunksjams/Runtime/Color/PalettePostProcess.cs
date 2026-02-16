﻿﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PalettePostProcess : MonoBehaviour
{
    public ColorPalette palette;
    [Range(8, 64)] public int lutSize = 16;

    public Shader paletteShader;
    Material _material;
    Texture2D _currentLut;
    ColorPalette _lastPalette;

    void OnEnable()
    {
        if (paletteShader == null) paletteShader = Shader.Find("Hidden/DunksJams/PaletteQuantize");
        if (paletteShader != null) _material = new Material(paletteShader);
    }

    void OnDisable()
    {
        if (_material) Destroy(_material);
        _material = null;
        // Do not destroy cached LUT textures - PaletteUtils owns the cache.
        _currentLut = null;
    }

    void UpdateLutIfNeeded()
    {
        // Resolve palette: explicit component palette wins, otherwise use global default
        var resolved = palette != null ? palette : PaletteSettings.Load()?.defaultPalette;

        if (resolved == _lastPalette && _currentLut != null) return;

        // Do not destroy cached LUTs. Request LUT from PaletteUtils cache.
        if (resolved == null)
        {
            _currentLut = null;
            _lastPalette = null;
            return;
        }

        _currentLut = PaletteUtils.PaletteToLut(resolved, lutSize);
        _lastPalette = resolved;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!_material)
        {
            Graphics.Blit(src, dest);
            return;
        }

        UpdateLutIfNeeded();
        if (_currentLut == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        _material.SetTexture("_LutTex", _currentLut);
        _material.SetFloat("_LutSize", lutSize);
        Graphics.Blit(src, dest, _material);
    }
}