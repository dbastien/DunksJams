using System;
using System.Numerics;

public static class FFT
{
    /// <summary>
    /// Compute the FFT of a real-valued signal
    /// Returns complex frequency domain data
    /// </summary>
    public static Complex[] Forward(float[] realInput)
    {
        int n = realInput.Length;
        Complex[] complexInput = new Complex[n];

        // Convert real input to complex
        for (int i = 0; i < n; i++)
        {
            complexInput[i] = new Complex(realInput[i], 0);
        }

        return Forward(complexInput);
    }

    /// <summary>
    /// Compute the FFT of complex input
    /// </summary>
    public static Complex[] Forward(Complex[] input)
    {
        int n = input.Length;
        Complex[] output = new Complex[n];
        Array.Copy(input, output, n);

        // Cooley-Tukey FFT algorithm
        FFTInternal(output, false);

        return output;
    }

    /// <summary>
    /// Compute the inverse FFT
    /// </summary>
    public static Complex[] Inverse(Complex[] input)
    {
        int n = input.Length;
        Complex[] output = new Complex[n];

        // Conjugate input for inverse
        for (int i = 0; i < n; i++)
        {
            output[i] = Complex.Conjugate(input[i]);
        }

        FFTInternal(output, false);

        // Conjugate and scale result
        for (int i = 0; i < n; ++i)
        {
            output[i] = Complex.Conjugate(output[i]) / n;
        }

        return output;
    }

    /// <summary>
    /// Convert complex FFT output to magnitude spectrum
    /// </summary>
    public static float[] MagnitudeSpectrum(Complex[] fftData)
    {
        float[] magnitudes = new float[fftData.Length / 2];

        for (int i = 0; i < magnitudes.Length; ++i)
        {
            magnitudes[i] = (float)fftData[i].Magnitude;
        }

        return magnitudes;
    }

    /// <summary>
    /// Convert complex FFT output to magnitude spectrum in dB
    /// </summary>
    public static float[] MagnitudeSpectrumDB(Complex[] fftData, float reference = 1f)
    {
        float[] magnitudes = MagnitudeSpectrum(fftData);
        float[] dbMagnitudes = new float[magnitudes.Length];

        for (int i = 0; i < magnitudes.Length; i++)
        {
            if (magnitudes[i] > 0)
            {
                dbMagnitudes[i] = 20f * MathF.Log10(magnitudes[i] / reference);
            }
            else
            {
                dbMagnitudes[i] = -80f; // Silence threshold
            }
        }

        return dbMagnitudes;
    }

    // Internal FFT implementation using Cooley-Tukey algorithm
    private static void FFTInternal(Complex[] data, bool inverse)
    {
        //TBD
    }
}