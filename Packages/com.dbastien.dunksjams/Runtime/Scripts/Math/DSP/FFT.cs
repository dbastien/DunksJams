using System;
using System.Numerics;

public static class FFT
{
    public static Complex[] Forward(float[] realInput)
    {
        var n = realInput.Length;
        var complexInput = new Complex[n];

        // Convert real input to complex
        for (var i = 0; i < n; i++) complexInput[i] = new Complex(realInput[i], 0);

        return Forward(complexInput);
    }

    public static Complex[] Forward(Complex[] input)
    {
        var n = input.Length;
        var output = new Complex[n];
        Array.Copy(input, output, n);

        // Cooley-Tukey FFT algorithm
        FFTInternal(output, false);

        return output;
    }

    public static Complex[] Inverse(Complex[] input)
    {
        var n = input.Length;
        var output = new Complex[n];

        // Conjugate input for inverse
        for (var i = 0; i < n; i++) output[i] = Complex.Conjugate(input[i]);

        FFTInternal(output, false);

        // Conjugate and scale result
        for (var i = 0; i < n; ++i) output[i] = Complex.Conjugate(output[i]) / n;

        return output;
    }

    public static float[] MagnitudeSpectrum(Complex[] fftData)
    {
        var magnitudes = new float[fftData.Length / 2];

        for (var i = 0; i < magnitudes.Length; ++i) magnitudes[i] = (float)fftData[i].Magnitude;

        return magnitudes;
    }

    public static float[] MagnitudeSpectrumDB(Complex[] fftData, float reference = 1f)
    {
        var magnitudes = MagnitudeSpectrum(fftData);
        var dbMagnitudes = new float[magnitudes.Length];

        for (var i = 0; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > 0)
                dbMagnitudes[i] = 20f * MathF.Log10(magnitudes[i] / reference);
            else
                dbMagnitudes[i] = -80f; // Silence threshold
        }

        return dbMagnitudes;
    }

    static void FFTInternal(Complex[] data, bool inverse)
    {
        var n = data.Length;
        if (n <= 1) return;
        if ((n & (n - 1)) != 0) throw new ArgumentException("FFT length must be a power of two.", nameof(data));

        for (int i = 1, j = 0; i < n; ++i)
        {
            var bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (data[i], data[j]) = (data[j], data[i]);
        }

        var sign = inverse ? 1f : -1f;

        for (var len = 2; len <= n; len <<= 1)
        {
            var angle = sign * 2f * MathF.PI / len;
            var wLen = new Complex(MathF.Cos(angle), MathF.Sin(angle));
            var halfLen = len >> 1;

            for (var i = 0; i < n; i += len)
            {
                var w = Complex.One;
                for (var j = 0; j < halfLen; ++j)
                {
                    var u = data[i + j];
                    var v = w * data[i + j + halfLen];
                    data[i + j] = u + v;
                    data[i + j + halfLen] = u - v;
                    w *= wLen;
                }
            }
        }

        if (inverse)
        {
            var invN = 1.0 / n;
            for (var i = 0; i < n; ++i) data[i] *= invN;
        }
    }
}