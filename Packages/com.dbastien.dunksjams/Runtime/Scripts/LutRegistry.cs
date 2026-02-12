using UnityEngine;

/// <summary>
/// Registry of discovered LUT textures for palette color grading.
/// Populated by LutRegistryBuilder (Editor menu).
/// </summary>
public class LutRegistry : ScriptableObject
{
    [SerializeField] Texture2D[] _luts = {};
    [SerializeField] string[] _names = {};

    public int Count => _luts?.Length ?? 0;
    public Texture2D this[int i] => _luts != null && i >= 0 && i < _luts.Length ? _luts[i] : null;
    public string GetName(int i) => _names != null && i >= 0 && i < _names.Length ? _names[i] : null;

    public bool TryGetLut(string name, out Texture2D lut)
    {
        lut = null;
        if (_names == null || _luts == null) return false;
        for (var i = 0; i < _names.Length; i++)
        {
            if (_names[i] == name) { lut = _luts[i]; return true; }
        }
        return false;
    }

    public void SetEntries(Texture2D[] luts, string[] names)
    {
        _luts = luts ?? System.Array.Empty<Texture2D>();
        _names = names ?? System.Array.Empty<string>();
        if (_names.Length != _luts.Length)
        {
            _names = new string[_luts.Length];
            for (var i = 0; i < _luts.Length; i++)
                _names[i] = _luts[i] ? _luts[i].name : "";
        }
    }
}
