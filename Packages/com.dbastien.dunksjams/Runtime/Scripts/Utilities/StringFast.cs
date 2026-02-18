using System.Collections.Generic;

/// <summary>
/// Mutable string class optimized for speed and minimal allocations.
/// Similar to StringBuilder but avoids allocations for int/float Append.
/// 
/// Original author: Nicolas Gadenne (contact@gaddygames.com).
/// Includes time formatting extensions.
/// </summary>
public class StringFast
{
    private string _generated = "";
    private bool _isGenerated;
    private char[] _buffer;
    private int _pos;
    private int _capacity;
    private List<char> _replacement;

    private object _valueControl;
    private int _valueControlInt = int.MinValue;

    public StringFast(int initialCapacity = 32) =>
        _buffer = new char[_capacity = initialCapacity];

    public override string ToString()
    {
        if (!_isGenerated)
        {
            _generated = new string(_buffer, 0, _pos);
            _isGenerated = true;
        }

        return _generated;
    }

    public bool IsEmpty => _isGenerated ? _generated == null : _pos == 0;

    // ========================================================================
    // Value change detection
    // ========================================================================

    public bool IsModified(int newValue)
    {
        bool changed = newValue != _valueControlInt;
        if (changed) _valueControlInt = newValue;
        return changed;
    }

    public bool IsModified(object newValue)
    {
        bool changed = !newValue.Equals(_valueControl);
        if (changed) _valueControl = newValue;
        return changed;
    }

    // ========================================================================
    // Set methods
    // ========================================================================

    public void Set(string str)
    {
        Clear();
        Append(str);
        _generated = str;
        _isGenerated = true;
    }

    public void Set(object str) => Set(str.ToString());

    public void Set<T1, T2>(T1 a, T2 b)
    {
        Clear();
        Append(a);
        Append(b);
    }

    public void Set<T1, T2, T3>(T1 a, T2 b, T3 c)
    {
        Clear();
        Append(a);
        Append(b);
        Append(c);
    }

    public void Set<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d)
    {
        Clear();
        Append(a);
        Append(b);
        Append(c);
        Append(d);
    }

    public void Set(params object[] parts)
    {
        Clear();
        for (var i = 0; i < parts.Length; i++) Append(parts[i]);
    }

    // ========================================================================
    // Append methods (allocation-free for string/int/float)
    // ========================================================================

    public StringFast Clear()
    {
        _pos = 0;
        _isGenerated = false;
        return this;
    }

    public StringFast Append(string value)
    {
        EnsureCapacity(value.Length);
        for (var i = 0; i < value.Length; i++)
            _buffer[_pos + i] = value[i];
        _pos += value.Length;
        _isGenerated = false;
        return this;
    }

    public StringFast Append(object value) => Append(value.ToString());

    public StringFast Append(int value)
    {
        EnsureCapacity(16);
        if (value < 0)
        {
            value = -value;
            _buffer[_pos++] = '-';
        }

        var nbChars = 0;
        do
        {
            _buffer[_pos++] = (char)('0' + value % 10);
            value /= 10;
            nbChars++;
        }
        while (value != 0);

        Reverse(nbChars);
        _isGenerated = false;
        return this;
    }

    public StringFast Append(float valueF)
    {
        double value = valueF;
        _isGenerated = false;
        EnsureCapacity(32);

        if (value == 0)
        {
            _buffer[_pos++] = '0';
            return this;
        }

        if (value < 0)
        {
            value = -value;
            _buffer[_pos++] = '-';
        }

        var nbDecimals = 0;
        while (value < 1000000)
        {
            value *= 10;
            nbDecimals++;
        }

        var valueLong = (long)System.Math.Round(value);

        var nbChars = 0;
        var isLeadingZero = true;
        while (valueLong != 0 || nbDecimals >= 0)
        {
            if (valueLong % 10 != 0 || nbDecimals <= 0) isLeadingZero = false;
            if (!isLeadingZero) _buffer[_pos + nbChars++] = (char)('0' + valueLong % 10);
            if (--nbDecimals == 0 && !isLeadingZero) _buffer[_pos + nbChars++] = '.';
            valueLong /= 10;
        }

        _pos += nbChars;

        Reverse(nbChars);
        return this;
    }

    // ========================================================================
    // Replace
    // ========================================================================

    public StringFast Replace(string oldStr, string newStr)
    {
        if (_pos == 0) return this;
        _replacement ??= new List<char>();

        for (var i = 0; i < _pos; i++)
        {
            var match = false;
            if (_buffer[i] == oldStr[0])
            {
                var k = 1;
                while (k < oldStr.Length && i + k < _pos && _buffer[i + k] == oldStr[k]) k++;
                match = k >= oldStr.Length;
            }

            if (match)
            {
                i += oldStr.Length - 1;
                if (newStr != null)
                    for (var k = 0; k < newStr.Length; k++)
                        _replacement.Add(newStr[k]);
            }
            else { _replacement.Add(_buffer[i]); }
        }

        EnsureCapacity(_replacement.Count - _pos);
        for (var k = 0; k < _replacement.Count; k++)
            _buffer[k] = _replacement[k];
        _pos = _replacement.Count;
        _replacement.Clear();
        _isGenerated = false;
        return this;
    }

    // ========================================================================
    // Time Formatting Helpers
    // ========================================================================

    public static string FormatTime(int hours, int minutes)
    {
        var sf = new StringFast(6);
        AppendPaddedTwo(sf, hours, 99);
        sf.Append(":");
        AppendPaddedTwo(sf, minutes, 59);
        return sf.ToString();
    }

    public static string FormatTime(int hours, int minutes, int secs)
    {
        var sf = new StringFast(15);
        AppendPaddedTwo(sf, hours, 99);
        sf.Append(":");
        AppendPaddedTwo(sf, minutes, 59);
        sf.Append(":");
        AppendPaddedTwo(sf, secs, 59);
        return sf.ToString();
    }

    public static string FormatTime(int hours, int minutes, int secs, int msecs, bool showTenths)
    {
        var sf = new StringFast(15);
        AppendPaddedTwo(sf, hours, 99);
        sf.Append(":");
        AppendPaddedTwo(sf, minutes, 59);
        sf.Append(":");
        AppendPaddedTwo(sf, secs, 59);
        sf.Append(".");
        if (showTenths) AppendPaddedTwo(sf, msecs, 99);
        else AppendPaddedThree(sf, msecs);
        return sf.ToString();
    }

    // ========================================================================
    // Private Helpers
    // ========================================================================

    private void EnsureCapacity(int charsToAdd)
    {
        if (_pos + charsToAdd <= _capacity) return;
        _capacity = System.Math.Max(_capacity + charsToAdd, _capacity * 2);
        var newBuf = new char[_capacity];
        _buffer.CopyTo(newBuf, 0);
        _buffer = newBuf;
    }

    private void Reverse(int nbChars)
    {
        for (int i = nbChars / 2 - 1; i >= 0; i--)
        {
            char c = _buffer[_pos - i - 1];
            _buffer[_pos - i - 1] = _buffer[_pos - nbChars + i];
            _buffer[_pos - nbChars + i] = c;
        }
    }

    private static void AppendPaddedTwo(StringFast sf, int val, int max)
    {
        if (val > max) { sf.Append("--"); }
        else if (val >= 10) { sf.Append(val); }
        else
        {
            sf.Append("0");
            sf.Append(val);
        }
    }

    private static void AppendPaddedThree(StringFast sf, int val)
    {
        if (val > 999) { sf.Append("---"); }
        else if (val > 99) { sf.Append(val); }
        else if (val > 9)
        {
            sf.Append("0");
            sf.Append(val);
        }
        else
        {
            sf.Append("00");
            sf.Append(val);
        }
    }
}